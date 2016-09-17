using System.Collections.ObjectModel;
using DDT.Application.Services;
using DDT.Core.Descent;
using System.Windows.Threading;
using System;
using System.Linq;
using DDT.Application.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DDT.Application.ViewModels
{
    public partial class MainViewModel
    {
        partial void InitializeObtainResults()
        {
            Results = new ObservableCollection<DiscoveryTargetWrapper>();
        }

        public ObservableCollection<DiscoveryTargetWrapper> Results { get; private set; }

        private DispatcherTimer _getResultsTimer;
        private DateTime? _getResultsTimestamp;
        private Guid _operation;

        private string _keywordResults;
        public string KeywordResults
        {
            get { return _keywordResults; }
            set
            {
                _keywordResults = value;
                RaisePropertyChanged(nameof(KeywordResults));
            }
        }

        private bool SearchResults(object o)
        {
            if (o is DeploymentItem)
            {
                var dep = (DeploymentItem)o;
                return dep.Name.ToLower().Contains(KeywordResults.ToLower())
                || dep.OS.ToLower().Contains(KeywordResults.ToLower())
                || dep.IP.ToLower().Contains(KeywordResults.ToLower());
            }

            return false;
        }

        private async Task ResolveAggregatedDeploymentItems()
        {
            KeywordResults = null;
            StopAvailable = true;
            GenerateReportAvailable = false;
            ProgressMessage = "Discovering Computers...";
            Progress = 100;

            Results = new ObservableCollection<DiscoveryTargetWrapper>();
            RaisePropertyChanged(nameof(Results));
            StopDeployCommand.InvalidateCanExecute();

            AggregatedDeploymentItemsResolvedEventArgs resolved = null;
            if (HasNetworkAddress)
            {
                var result = await TryAction(_discoveryService.GetByIpAddresses(NetworkAddresses));
                if (result.Exception == null)
                {
                    NetworkAddressItems.Clear();
                    NetworkAddressItems.AddRange(result.Items);

                    await Task.Delay(1500);

                    resolved = await TryAction(_discoveryService.ResolveAggregatedDeploymentItems(
                        NetworkAddressItems,
                        HasWorkgroup ? GetSelectedWorkgroupItems() : new List<DeploymentItem>(),
                        HasActiveDirectory ? GetSelectedActiveDirectoryItems() : new List<DeploymentItem>(),
                        ActiveDirectoryLogin, 
                        ActiveDirectoryPassword));
                }
                else
                {
                    StopAvailable = false;
                    Progress = 0;
                }
            }
            else
            {
                await Task.Delay(1500);

                resolved = await TryAction(_discoveryService.ResolveAggregatedDeploymentItems(
                    new List<DeploymentItem>(),
                    HasWorkgroup ? GetSelectedWorkgroupItems() : new List<DeploymentItem>(),
                    HasActiveDirectory ? GetSelectedActiveDirectoryItems() : new List<DeploymentItem>(),
                    ActiveDirectoryLogin, 
                    ActiveDirectoryPassword));
            }

            await OnAggregatedDeploymentItemsResolved(resolved);
        }

        private async Task OnAggregatedDeploymentItemsResolved(AggregatedDeploymentItemsResolvedEventArgs e)
        {
            if (e.Exception != null)
            {
                StopAvailable = false;
                Progress = 0;
                return;
            }

            Func<string, string> getPath = _ => _.Remove(0, _.IndexOf("] ") + 2);

            var installationParameters = IsInstallationParametersSpecified ? InstallationParameters : string.Empty;
            var packagePath = getPath(_package);
            var transformPath = !HasTransform ? null : getPath(_transform);
            var package = packagePath.ToLower().EndsWith("exe") ?
                InstallPackageInfo.CreateExePackageInfo(packagePath, installationParameters, TargetSystemType.x86)
                : InstallPackageInfo.CreateMsiPackageInfo(packagePath, transformPath, installationParameters, TargetSystemType.x86);

            InstallPackageInfo package64;
            if (SeparatePackages)
            {
                installationParameters = IsInstallationParameters64Specified ? InstallationParameters64 : installationParameters;
                var package64Path = getPath(_package64);
                var transform64Path = !HasTransform64 ? null : getPath(_transform64);
                package64 = package64Path.ToLower().EndsWith("exe")
                    ? InstallPackageInfo.CreateExePackageInfo(package64Path, installationParameters, TargetSystemType.x64)
                    : InstallPackageInfo.CreateMsiPackageInfo(package64Path, transform64Path, installationParameters, TargetSystemType.x64);
            }
            else
                package64 = package;

            var deployCollection = new Collection<DeploymentItem>();

            Action<DeploymentItem[]> fillDeployCollection = _ =>
            {
                foreach (var item in _)
                    if (!deployCollection.Any(__ => __.TargetUid == item.TargetUid))
                        deployCollection.Add(item);
            };

            if (e.ItemsByDomain != null && e.ItemsByDomain.Any())
            {
                fillDeployCollection(e.ItemsByDomain);
                Results.Add(new DiscoveryTargetWrapper
                {
                    Type = DiscoveryTargetType.ActiveDirectory,
                    Items = e.ItemsByDomain
                });
            }
            else if (HasActiveDirectory)
            {
                Results.Add(new DiscoveryTargetWrapper
                {
                    Type = DiscoveryTargetType.ActiveDirectory
                });
            }
            if (e.ItemsByWorkgroup != null && e.ItemsByWorkgroup.Any())
            {
                fillDeployCollection(e.ItemsByWorkgroup);
                Results.Add(new DiscoveryTargetWrapper
                {
                    Type = DiscoveryTargetType.Workgroup,
                    Items = e.ItemsByWorkgroup
                });
            }
            if (e.ItemsByIp != null && e.ItemsByIp.Any())
            {
                fillDeployCollection(e.ItemsByIp);
                Results.Add(new DiscoveryTargetWrapper
                {
                    Type = DiscoveryTargetType.NetworkAddress,
                    Items = e.ItemsByIp
                });
            }

            if (deployCollection.Any(_ => _.IsOnline))
            {
                var result = await TryAction(_deploymentService.BeginDeployment(deployCollection.ToArray(), 
                    Login, Password, TimeSpan.FromMinutes(DeployTimeout), package, package64));
                OnDeploymentStarted(result);
            }
            else
            {
                StopAvailable = false;
                Progress = 0;
            }

            foreach (var result in Results)
                if (!result.Items.Any(_ => _.IsOnline))
                    result.Value = "Completed with errors.";
        }

        private void OnDeploymentStarted(DeploymentStartedEventArgs e)
        {
            if (e.Exception != null)
            {
                StopAvailable = false;
                Progress = 0;
                return;
            }

            StopDeployCommand.InvalidateCanExecute();

            _operation = e.OperationUid;

            if(_getResultsTimer == null)
                _getResultsTimer = new DispatcherTimer(
                    TimeSpan.FromSeconds(0.5), 
                    DispatcherPriority.Background, 
                    _getResultsTimerIterate, 
                    App.Current.Dispatcher);

            if (!_getResultsTimer.IsEnabled)
                _getResultsTimer.Start();

            Progress = 0;
        }

        private void GetModifiedComputerResultsForOperationCompleted(GetModifiedComputerResultsForOperationEventArgs e)
        {
            if (e.Exception != null)
            {
                StopAvailable = false;
                return;
            }

            var results = e.Results;
            if (results != null && results.Any())
            {
                _getResultsTimestamp = results.Max(_ => _.ModifiedTime);
                var updatedDictionary = results.ToDictionary(r => r.TargetUid, r => r);
                foreach (var result in Results)
                {
                    if (result.Items == null)
                    {
                        result.Value = "No computers found.";
                        continue;
                    }

                    bool resultChanged = false;

                    foreach (var deploymentItem in result.Items)
                    {
                        ComputerDescentResultModel updated;
                        var exists = updatedDictionary.TryGetValue(deploymentItem.TargetUid, out updated);
                        if (exists)
                        {
                            resultChanged = true;
                            
                            deploymentItem.SystemKind = updated.Computer.SystemKind;
                            deploymentItem.PackageName = updated.PackageName;
                            deploymentItem.Is64 = updated.Is64;
                            deploymentItem.Percentage = updated.Percentage;

                            if (!deploymentItem.Stopping)
                                deploymentItem.Value = updated.ShortStatusMessage.ToString();

                            if (updated.Completed)
                            {
                                if (deploymentItem.Stopping)
                                {
                                    deploymentItem.Value = "Stopped.";
                                    continue;
                                }

                                if (updated.Succeeded)
                                {
                                    deploymentItem.Value = updated.ShortStatusMessage.ToString();
                                    deploymentItem.Succeded = true;
                                }
                                else
                                    deploymentItem.Failed = true;
                            }
                        }
                    }

                    if (resultChanged)
                    {
                        var averagePercetage = result.Items.Average(r => r.Percentage);
                        result.InProgress = averagePercetage > 0 && averagePercetage < 100;

                        if (result.Stopping)
                        {
                            if (averagePercetage == 100)
                                result.Value = "Stopped.";
                            continue;
                        }

                        if (averagePercetage < 100)
                        {
                            result.Value = averagePercetage == 0 ? "Deploying..." : $"[{averagePercetage:F2}%] Deploying...";
                            result.Value += $" ({result.Items.Count(_ => _.Succeded || _.Failed)}/{result.Items.Length})";
                        }
                        else
                        {
                            if (!result.Failed)
                            {
                                result.Succeded = true;
                                result.Value = "Successfully completed.";
                            }
                            else
                                result.Value = "Completed with errors.";
                        }
                    }
                }
            }

            if (e.OperationCompleted)
            {
                StopAvailable = false;
                GenerateReportAvailable = true;
            }
            else if (!_getResultsTimer.IsEnabled)
                _getResultsTimer.Start();
        }

        private void _getResultsTimerIterate(object sender, EventArgs e)
        {
            if (_operation != Guid.Empty)
            {
                if (_getResultsTimer.IsEnabled)
                    _getResultsTimer.Stop();
                GetModifiedComputerResultsForOperationCompleted(
                    _deploymentService.GetModifiedComputerResultsForOperation(_operation, _getResultsTimestamp));
            }
        }

        private void SetStoppingStatuses()
        {
            foreach (var result in Results)
            {
                if (result.Items == null)
                    continue;

                if (result.Succeded || result.Items == null || result.Items.All(_ => _.Succeded || _.Failed))
                    continue;

                result.Stopping = true;
                result.Value = "Stopping...";

                foreach (var deploymentItem in result.Items)
                {
                    if (deploymentItem.Succeded || deploymentItem.Failed)
                        continue;

                    deploymentItem.Stopping = true;
                    deploymentItem.Value = "Stopping...";
                }
            }
        }
    }
}