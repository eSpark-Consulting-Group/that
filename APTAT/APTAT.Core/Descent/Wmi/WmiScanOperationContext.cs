using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using APTAT.Common;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Messages;

namespace APTAT.Core.Descent.Wmi
{
    public class WmiScanOperationContext : DescentResult, IWmiScanOperationContext
    {
        private readonly Guid _sessionId = Guid.NewGuid();
        private readonly ConcurrentDictionary<Guid,WmiScanResult> _scanResults = new ConcurrentDictionary<Guid, WmiScanResult>(); 

        public WmiScanOperationContext(IEnumerable<IComputer> computers, IDevicesSourceDescription devicesSourceDescription)
            : base(computers, devicesSourceDescription)
        {
            var scanResultType = ScanResultType.InProgress;
            foreach (var target in computers)
            {
                _scanResults.TryAdd(
                       target.Uid,
                       new WmiScanResult()
                       {
                           HostName = target.Name,
                           IpAddress = target.IPAddress != null ? target.IPAddress.ToString() : String.Empty,
                           ResultType = scanResultType,
                           TargetUid = target.Uid,
                           OrganizationUnit = target.OrganizationUnit
                       });

            }

        }

        private readonly object _sync = new object();

        public ConcurrentDictionary<Guid, WmiScanResult> ScanResults
        {
            get { return _scanResults; }
        }

        public override void Changed(params ChangesTrackingItem[] items)
        {
            base.Changed(items);
            foreach (var item in items)
            {
                if (item.ChangeType == ChangeType.Changed && item.ChangedItem is IDescentProgressItem)
                {
                    DescentProgressItem descentProgressItem = (DescentProgressItem)item.ChangedItem;
                    WmiScanResult result;
                    if (_scanResults.TryGetValue(descentProgressItem.ComputerUid, out result))
                    {
                        if (result != null)
                        {
                            lock (_sync)
                            {
                                result.ProgressPercenage = descentProgressItem.ProgressPercentage;
                            }
                        }
                    }
                }
            }

            OnOperationProgressChanged(new OperationProgressChangedEventArgs() { Percentage = CalculateTotalPercentage(), OperationUid = OperationUid });
        }

        public override void OnDescendOperationEvent(bool ok, Guid computerUid, FormattedText errorMessage, FormattedText shortStatusMessage,
            string errorCode, int systemCode, string systemMessage, bool isCanceled = false)
        {
            if (!ok)
            {
                WmiScanResult result;
                if (_scanResults.TryGetValue(computerUid, out result))
                {
                    if (result != null)
                    {
                        lock (_sync)
                        {
                            ScanResultType scanResultType = ScanResultType.Failed;
                            if (errorCode == "status_operation_canceled")
                                scanResultType = ScanResultType.Canceled;
                            result.ResultErrorText = systemMessage;
                            result.ResultType = scanResultType;
                        }
                    }
                }
            }
            
            base.OnDescendOperationEvent(ok, computerUid, errorMessage, shortStatusMessage, 
                errorCode, systemCode, systemMessage, isCanceled);
        }

        public override Guid SessionId
        {
            get { return _sessionId; }
        }

        private double CalculateTotalPercentage()
        {
            var onlineScanResults = (from result in _scanResults.Values
                                     where result.ResultType != ScanResultType.NotPerformed
                                     select result).ToArray();

            var total = onlineScanResults.Count() * 100;
            var current = onlineScanResults.Sum(scanResult => scanResult.ProgressPercenage);

            return (double)current / total * 100d;
        }

        public event EventHandler<OperationProgressChangedEventArgs> OperationProgressChanged;
        private void OnOperationProgressChanged(OperationProgressChangedEventArgs args)
        {
            EventHandler<OperationProgressChangedEventArgs> temp = OperationProgressChanged;
            if (temp != null)
                temp(this, args);
        }
    }
}
