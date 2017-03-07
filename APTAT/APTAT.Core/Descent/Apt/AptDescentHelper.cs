using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using APTAT.DeploymentCore.Descent.Exceptions;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Descent.Helpers;
using APTAT.DeploymentCore.Descent.Helpers.Transport;

namespace APTAT.Core.Descent.Apt
{
	namespace Transport
	{

		public class AptDescentHelper : DescentHelper
		{

            //private const string ScannerServiceX86FileName = "ScannerService.exe";
            //private const string ScannerServiceX64FileName = "ScannerService64.exe";
            private const string ScannerServiceX86FileName = "cmdapt86.exe";
            private const string ScannerServiceX64FileName = "cmdapt64.exe";
            
			private const string ScanStatusFileName = "scan_status.txt";
            private const string ScanResultsFileName = "out.xml";
            private readonly string TvlFileName;
            private readonly string TrlFileName;
            private readonly string TvlFileLocalPath;
            private readonly string TrlFileLocalPath;

            private const string ScopeParamFormat = "--scope \"{0}\"";
            private const string OutputFileParamFormat = "--output \"{0}\"";
            private const string StatusFileParamFormat = "--status \"{0}\"";
            private const string TvlParamFormat = "--tvl \"{0}\"";
            private const string TrlParamFormat = "--trl \"{0}\"";
            private const string TcpPortParamFormat = "--flsTcpPort {0}";
            private const string UdpPortParamFormat = "--flsUdpPort {0}";
            private const string SkipGacParam = "--skipGAC";
            
		
            private string _responseFilePathForCurrentComputer;
            private string _resultsFile;
		    private string _tvlFile;
            private string _trlFile;
            private readonly string[] _additionFiles;

            private const string AptScannerTempFolderName = "ComodoAptAtScanner";
       

            
            protected override string[] GetAdditionalFilesToCopy(bool isX64)
            {
                 return _additionFiles; 
            }

		    protected override string ServiceName { get {return "ScannerService.exe"; } }
            protected override string ServiceDisplayName { get { return "Comodo APT Scanner"; } }

            public AptDescentHelper( RemoteHostInfo info, IScanOptions options, CancellationToken cancellationToken)
                : base (info, cancellationToken)
			{
                var folderPath = new DirectoryInfo(System.Reflection.Assembly.GetEntryAssembly().Location)
                    .Parent.FullName;
                TvlFileName = "tvl.txt";
                TrlFileName = "trl.txt";
                TrlFileLocalPath = folderPath + "\\" + TrlFileName;
                TvlFileLocalPath = folderPath + "\\" + TvlFileName;
                _additionFiles = new[] { TvlFileLocalPath, TrlFileLocalPath };
                _options = options;
			}

			protected override void InitPaths()
			{
                base.InitPaths();
			    TempFolderName = AptScannerTempFolderName;
                RemoteWorkingDirectory = RemoteAdminPath + @"\" + AptScannerTempFolderName;
                _responseFilePathForCurrentComputer = RemoteWorkingDirectory + @"\" + ScanStatusFileName;
                _resultsFile = RemoteWorkingDirectory + @"\" + ScanResultsFileName;
                _tvlFile = RemoteWorkingDirectory + @"\" + TvlFileName;
                _trlFile = RemoteWorkingDirectory + @"\" + TrlFileName;
			}

			
            //======================================================================
            public class AptEventArgs : DescentHelperEventArgs
            {
                public AptEventArgs(Guid targetUid)
                    :base(targetUid)
                {
                    
                }
                public string HostName { get; set; }
                public int EnumeratedFilesCount { get; set; }
                public long EnumeratedFilesSize { get; set; }
            }

            public class AptScanningEventArgs : AptEventArgs
            {
                public AptScanningEventArgs(Guid targetUid)
                    :base(targetUid)
                {

                }
                public int TotalFilesScanned { get; set; }
                public int UnknownFilesFound { get; set; }
                public int SafeFilesFound { get; set; }
                public int MalwaresFound { get; set; }
                public int EmptyFilesFound { get; set; }
                public int SkippedFilesCount { get; set; }
                public int FailedToAnalyzeFilesCount { get; set; }
                public long TotalFilesSize { get; set; }
                public int NoVerdictFilesCount { get; set; }
                public double Percentage
                {
                    get
                    {
                        return EnumeratedFilesCount == 0? 100d: 
                            (double)TotalFilesScanned / EnumeratedFilesCount * 100d;
                    }
                }
            }

            public EventHandler<ScanProgressChangedEventArgs> ScanProgressChanged;
            private void InvokeScanProgressChanged(ScanProgressChangedEventArgs e)
            {
                EventHandler<ScanProgressChangedEventArgs> temp = ScanProgressChanged;
                if (temp != null)
                    temp(this, e);
            }
            public class ScanProgressChangedEventArgs : AptScanningEventArgs
            {
                public ScanProgressChangedEventArgs(Guid targetUid)
                    :base(targetUid)
                {

                }
            }

            public EventHandler<ScanFailedEventArgs> ScanFailed;
            private void InvokeScanFailed(ScanFailedEventArgs e)
            {
                EventHandler<ScanFailedEventArgs> temp = ScanFailed;
                if (temp != null)
                    temp(this, e);
            }
            public class ScanFailedEventArgs : AptScanningEventArgs
            {
                public DescentStatus Status { get; set; }

                public ScanFailedEventArgs(Guid targetUid) : base(targetUid)
                {
                }
            }

            public EventHandler<ScanCompletedEventArgs> ScanCompleted;
            private void InvokeScanCompleted(ScanCompletedEventArgs e)
            {
                EventHandler<ScanCompletedEventArgs> temp = ScanCompleted;
                if (temp != null)
                    temp(this, e);
            }
            public class ScanCompletedEventArgs : AptScanningEventArgs
            {
                public ScanCompletedEventArgs(Guid targetUid) : base(targetUid)
                {
                }
            }

            public EventHandler<ScanResultsFileReceivedEventArgs> ScanResultsFileReceived;
            private void InvokeScanResultsFileReceived(ScanResultsFileReceivedEventArgs e)
            {
                EventHandler<ScanResultsFileReceivedEventArgs> temp = ScanResultsFileReceived;
                if (temp != null)
                    temp(this, e);
            }
            public class ScanResultsFileReceivedEventArgs : AptScanningEventArgs
            {
                public string FileName { get; set; }

                public ScanResultsFileReceivedEventArgs(Guid targetUid) : base(targetUid)
                {
                }
            }

            public EventHandler<DescentHelperEventArgs> ReceivingResults;
            private void InvokeReceivingResults(DescentHelperEventArgs e)
            {
                var temp = ReceivingResults;
                if (temp != null)
                    temp(this, e);
            }

            public EventHandler<AptEnumeratingEventArgs> EnumerationProgressChanged;
            private void InvokeEnumerationProgressChanged(AptEnumeratingEventArgs e)
            {
                EventHandler<AptEnumeratingEventArgs> temp = EnumerationProgressChanged;
                if (temp != null)
                    temp(this, e);
            }

            public class AptEnumeratingEventArgs : AptEventArgs
            {
                public EnAptStage Stage { get; set; }

                public AptEnumeratingEventArgs(Guid targetUid) : base(targetUid)
                {
                }
            }
            //======================================================================

		    
            private readonly IScanOptions _options;

            protected override string GetPackageFileName(bool is64)
            {
                var folderPath = new DirectoryInfo(System.Reflection.Assembly.GetEntryAssembly().Location)
                    .Parent.FullName;
                if (is64)
                    return folderPath + "\\" + ScannerServiceX64FileName;
                return folderPath + "\\" + ScannerServiceX86FileName;
            }

            protected override string[] GetServiceParams()
            {
                var scanScope = GetScanScope(_options);
                List<string> serviceParams = new List<string>();
                serviceParams.Add(String.Format(ScopeParamFormat, scanScope));
                serviceParams.Add(String.Format(StatusFileParamFormat, _responseFilePathForCurrentComputer));
                serviceParams.Add(String.Format(OutputFileParamFormat, _resultsFile));
                serviceParams.Add(String.Format(TvlParamFormat, _tvlFile));
                serviceParams.Add(String.Format(TrlParamFormat, _trlFile));
                serviceParams.Add("--filter \"*\"");
                serviceParams.Add("--scanPeOnly on");
                serviceParams.Add(String.Format(UdpPortParamFormat, _options.UdpPort));
                serviceParams.Add(String.Format(TcpPortParamFormat, _options.TcpPort));
                
                if (Properties.Settings.Default.SkipGac)
                    serviceParams.Add(SkipGacParam);

                return serviceParams.ToArray();
            }

            private static string GetScanScope(IScanOptions options)
            {
                switch (options.ScanKind)
                {
                    case ScanKind.Quick:
                        return @"processes|drivers|autoruns"; 
                        
                    case ScanKind.Full:
                        return @"?:";

                    case ScanKind.Custom:
                        return GetScanRegionArgs(options);

                    default:
                        throw new InvalidOperationException("Not supported scan kind");                        
                }                
            }

            private static string GetScanRegionArgs(IScanOptions options)
            {
                List<string> regions = new List<string>();
                if (options.ScanProcesses) regions.Add("processes");
                if (options.ScanDrivers) regions.Add("drivers");
                if (options.ScanAutoruns) regions.Add("autoruns");
                regions.AddRange(options.ScanPaths);
                return String.Join("|", regions);
            }

            protected override void CleanUpBeforeActionStarted()
		    {
		        base.CleanUpBeforeActionStarted();
                if (File.Exists(_responseFilePathForCurrentComputer))
                    File.Delete(_responseFilePathForCurrentComputer);

                // Delete response file if it exists
                if (File.Exists(_resultsFile))
                    File.Delete(_resultsFile);
            }

		    public override void ExecuteAction( string actionName, string[] args, int timeoutInMs )
			{
                base.ExecuteAction(actionName, args, timeoutInMs);

                int findFileAttemptsCount = 20;
                int readFileAttemptsCount = 20;
			    int checkFileHeaderAttemptsCount = 20;
                while (true/*attemptsCount > 0*/)
                {
                    if (findFileAttemptsCount > 0)
                        --findFileAttemptsCount;

                    try
                    {
                        if (CancellationToken.IsCancellationRequested || RemoteHostInfo.IsCancelled)
                            throw new OperationCanceledException();

                        bool? isServiceRunning = ServiceHelper.IsServiceRunning();
                        using (FileStream fs = new FileStream(_responseFilePathForCurrentComputer, FileMode.Open,
                            FileAccess.Read, FileShare.ReadWrite))
                        {
                            byte[] buf;
                            try
                            {
                                buf = new byte[fs.Length];
                                fs.Read(buf, 0, (int) fs.Length);
                            }
                            catch (ArgumentException)
                            {
                                buf = new byte[0];
                            }

                            string data = Encoding.Unicode.GetString(buf);
                            if (String.IsNullOrEmpty(data))
                            {
                                if (readFileAttemptsCount > 0)
                                    --readFileAttemptsCount;
                                findFileAttemptsCount = 20;
                                throw new StatusFileEmptyException();
                            }
                            readFileAttemptsCount = 20;

                            string[] temp = data.Split(';');
                            if (temp.Length != 14 && temp.Length != 13)
                            {
                                if (checkFileHeaderAttemptsCount > 0)
                                {
                                    --checkFileHeaderAttemptsCount;
                                }
                                findFileAttemptsCount = 20;
                                throw new ResponseFileHeaderInvalidException();
                                //throw new Exception("Response file header is not valid");
                            }
                            checkFileHeaderAttemptsCount = 20;

                            int statusInt;
                            Int32.TryParse(temp[0], out statusInt);

                            int enumeratedCount;
                            Int32.TryParse(temp[1], out enumeratedCount);

                            long enumeratedSize;
                            Int64.TryParse(temp[2], out enumeratedSize);

                            int totalFiles;
                            Int32.TryParse(temp[3], out totalFiles); //scanned count

                            long totalFilesSize;
                            Int64.TryParse(temp[4], out totalFilesSize); //scanned size

                            int unknownFiles;
                            Int32.TryParse(temp[5], out unknownFiles);

                            int newFiles;
                            Int32.TryParse(temp[6], out newFiles); //nonExistentCount

                            int errorFiles;
                            Int32.TryParse(temp[7], out errorFiles); //failed to analyze count

                            int emptyFiles;
                            Int32.TryParse(temp[8], out emptyFiles); //inaccessibleCount

                            int safeFiles;
                            Int32.TryParse(temp[9], out safeFiles);

                            int malwares;
                            Int32.TryParse(temp[10], out malwares);

                            int skipped;
                            Int32.TryParse(temp[11], out skipped);

                            string error = String.Empty;

                            int failedAtNodeCount = 0;
                            if (temp.Length == 14) //next FLS version
                            {
                                Int32.TryParse(temp[12], out failedAtNodeCount);
                                error = temp[13];
                            }
                            else if (temp.Length == 13)
                            {
                                error = temp[12];
                            }

                            EnAptStage status = (EnAptStage) statusInt;

                            if (status != EnAptStage.EnApt_Complete)
                            {
                                findFileAttemptsCount = 20;
                            }

                            if (isServiceRunning.HasValue && !isServiceRunning.Value && status != EnAptStage.EnApt_Complete &&
                                status != EnAptStage.EnApt_Failed)
                                throw new DescentException(DescentStatus.status_error, "scanner was crashed.", 0);

                            int unknownFilesTotal = unknownFiles + newFiles;


                            if (status == EnAptStage.EnApt_Pending)
                            {

                            }
                            else if (status == EnAptStage.EnApt_EnumeratingProcesses)
                            {
                                InvokeEnumerationProgressChanged(new AptEnumeratingEventArgs(RemoteHostInfo.TargetUid)
                                {
                                    EnumeratedFilesCount = enumeratedCount,
                                    EnumeratedFilesSize = enumeratedSize,
                                    HostName = RemoteHostInfo.HostName,
                                    Stage = status
                                });
                            }
                            else if (status == EnAptStage.EnApt_EnumeratingDrivers)
                            {
                                InvokeEnumerationProgressChanged(new AptEnumeratingEventArgs(RemoteHostInfo.TargetUid)
                                {
                                    EnumeratedFilesCount = enumeratedCount,
                                    EnumeratedFilesSize = enumeratedSize,
                                    HostName = RemoteHostInfo.HostName,
                                    Stage = status
                                });
                            }
                            else if (status == EnAptStage.EnApt_EnumeratingAutoruns)
                            {
                                InvokeEnumerationProgressChanged(new AptEnumeratingEventArgs(RemoteHostInfo.TargetUid)
                                {
                                    EnumeratedFilesCount = enumeratedCount,
                                    EnumeratedFilesSize = enumeratedSize,
                                    HostName = RemoteHostInfo.HostName,
                                    Stage = status
                                });
                            }
                            else if (status == EnAptStage.EnApt_EnumeratingFolder)
                            {
                                InvokeEnumerationProgressChanged(new AptEnumeratingEventArgs(RemoteHostInfo.TargetUid)
                                {
                                    EnumeratedFilesCount = enumeratedCount,
                                    EnumeratedFilesSize = enumeratedSize,
                                    HostName = RemoteHostInfo.HostName,
                                    Stage = status
                                });
                            }
                            else if (status == EnAptStage.EnApt_ScaningModules)
                            {
                                InvokeScanProgressChanged(new ScanProgressChangedEventArgs(RemoteHostInfo.TargetUid)
                                {
                                    HostName = RemoteHostInfo.HostName,
                                    EnumeratedFilesCount = enumeratedCount,
                                    EnumeratedFilesSize = enumeratedSize,
                                    TotalFilesScanned = totalFiles - skipped,
                                    UnknownFilesFound = unknownFilesTotal,
                                    SafeFilesFound = safeFiles,
                                    MalwaresFound = malwares,
                                    SkippedFilesCount = skipped,
                                    FailedToAnalyzeFilesCount = errorFiles,
                                    NoVerdictFilesCount = failedAtNodeCount
                                });
                            }
                            else if (status == EnAptStage.EnApt_WritingReport)
                            {

                            }
                            else if (status == EnAptStage.EnApt_Complete)
                            {
                                _response.Status = DescentStatus.status_success; //(DescentStatus)status;

                                InvokeScanCompleted(new ScanCompletedEventArgs(RemoteHostInfo.TargetUid)
                                {
                                    TotalFilesScanned = totalFiles - skipped,
                                    UnknownFilesFound = unknownFilesTotal,
                                    NoVerdictFilesCount = failedAtNodeCount,
                                    FailedToAnalyzeFilesCount = errorFiles,
                                    SkippedFilesCount = skipped
                                });

                                string resultsFileLocal = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                    String.Format("{0}_{1:yyyyMMddHHmmss}.xml", RemoteHostInfo.HostName, DateTime.Now));

                                InvokeReceivingResults(new DescentHelperEventArgs(RemoteHostInfo.TargetUid));

                                File.Copy(_resultsFile, resultsFileLocal, true);
                                //File.Delete(resultsFile);
                                InvokeScanResultsFileReceived(
                                    new ScanResultsFileReceivedEventArgs(RemoteHostInfo.TargetUid)
                                    {
                                        HostName = RemoteHostInfo.HostName,
                                        FileName = resultsFileLocal,
                                        EnumeratedFilesCount = enumeratedCount,
                                        EnumeratedFilesSize = enumeratedSize,
                                        TotalFilesScanned = totalFiles - skipped,
                                        UnknownFilesFound = unknownFilesTotal,
                                        SafeFilesFound = safeFiles,
                                        MalwaresFound = malwares,
                                        EmptyFilesFound = emptyFiles,
                                        SkippedFilesCount = skipped,
                                        FailedToAnalyzeFilesCount = errorFiles,
                                        NoVerdictFilesCount = failedAtNodeCount
                                    });
                                break;
                            }
                            else if (status == EnAptStage.EnApt_Failed)
                            {
                                string message = $"Error returned from a remote computer: {error}";

                                if (error.Contains("0x800705aa"))
                                {
                                    throw new DescentException(DescentStatus.status_insufficient_system_recources, message, 0);
                                }
                                else
                                {
                                    throw new DescentException(DescentStatus.status_error, message, 0);
                                }
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Unknown EnAptStage value: {0}", (int) status));
                            }
                            Thread.Sleep(3000);
                        }
                    }
                    catch (FileNotFoundException ex)
                    {
                        // If file is not found, we can sleep and try again
                        if (findFileAttemptsCount == 0)
                        {
                            // unless attempts count is zero...
                            const DescentStatus status = DescentStatus.status_remote_invalid_response;
                            InvokeScanFailed(new ScanFailedEventArgs(RemoteHostInfo.TargetUid)
                            {
                                Status = status,
                                HostName = RemoteHostInfo.HostName,
                            });
                            throw new DescentException(status, ex.Message, 0);
                        }
                        else
                        {
                            // sleep and try again
                            Thread.Sleep(1000);
                        }
                    }
                    catch (ResponseFileHeaderInvalidException ex)
                    {
                        // If file is not found, we can sleep and try again
                        if (checkFileHeaderAttemptsCount == 0)
                        {
                            // unless attempts count is zero...
                            const DescentStatus status = DescentStatus.status_remote_invalid_response;
                            InvokeScanFailed(new ScanFailedEventArgs(RemoteHostInfo.TargetUid)
                            {
                                Status = status,
                                HostName = RemoteHostInfo.HostName,
                            });
                            throw new DescentException(status, ex.Message, 0);
                        }
                        else
                        {
                            // sleep and try again
                            Thread.Sleep(1000);
                        }
                    }
                    catch (StatusFileEmptyException ex)
                    {
                        // If file is not found, we can sleep and try again
                        if (readFileAttemptsCount == 0)
                        {
                            // unless attempts count is zero...
                            const DescentStatus status = DescentStatus.status_remote_invalid_response;
                            InvokeScanFailed(new ScanFailedEventArgs(RemoteHostInfo.TargetUid)
                            {
                                Status = status,
                                HostName = RemoteHostInfo.HostName,
                            });
                            throw new DescentException(status, ex.Message, 0);
                        }
                        else
                        {
                            // sleep and try again
                            Thread.Sleep(1000);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw new ScanCanceledException();
                    }
                    catch (ThreadAbortException ex)
                    {
                        if ((ex.ExceptionState is bool) == true)
                        {
                            throw new ScanCanceledException();
                        }
                    }
                    catch (DescentException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        const DescentStatus status = DescentStatus.status_remote_invalid_response;
                        InvokeScanFailed(new ScanFailedEventArgs(RemoteHostInfo.TargetUid)
                        { Status = status, HostName = RemoteHostInfo.HostName});
                        throw new DescentException(status, ex.Message, 0);
                    }
                }
			}
            
		}
	}
}
