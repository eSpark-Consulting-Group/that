using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace APTAT.Core.Descent
{
    public class AptScanOperationContext : DescentResult, IAptScanOperationContext
    {
        private readonly Guid _sessionId = Guid.NewGuid();
        private readonly ConcurrentDictionary<Guid, ScanResult> _scanResults;

        public AptScanOperationContext(IEnumerable<TargetUidComputerPair> computers)
            : base(computers)
        {
            _scanResults = new ConcurrentDictionary<Guid, ScanResult>();
            var scanResultType = ScanResultType.InProgress;
            foreach (var target in computers)
            {
                var computer = target.Computer;
                _scanResults.TryAdd(
                       computer.Uid,
                       new ScanResult()
                       {
                           HostName = computer.Name,
                           IpAddress = computer.IPAddress != null ? computer.IPAddress.ToString() : String.Empty,
                           ResultType = scanResultType,
                           TargetUid = computer.Uid,
                           OrganizationUnit = computer.OrganizationUnit
                       });

            }
            
        }
        public DateTime Started { get { return StartTime; } }
        public int TotalTargetsCount { get; set; }
        public int NotPerformedTargetsCount { get; set; }
        public int SucceededTargetsCount { get; set; }
        public int FailedTargetsCount { get; set; }
        public int CanceledTargetsCount { get; set; }

        private readonly object _sync = new object();

        public ConcurrentDictionary<Guid, ScanResult> ScanResults
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
                    ScanResult result;
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

            OnOperationProgressChanged(new OperationProgressChangedEventArgs() { Percentage = CalculateTotalPercentage() });
        }

        public override void OnDescendOperationEvent(bool ok, IComputer computer, FormattedText errorMessage, FormattedText shortStatusMessage,
            string errorCode, int systemCode, string systemMessage)
        {
            if (!ok)
            {
                ScanResult result;
                if (_scanResults.TryGetValue(computer.Uid, out result))
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
            
            base.OnDescendOperationEvent(ok, computer, errorMessage, shortStatusMessage, errorCode, systemCode, systemMessage);
        }

        public override Guid SessionId
        {
            get { return _sessionId; }
        }

        private int CalculateTotalPercentage()
        {
            var onlineScanResults = (from result in _scanResults.Values
                                     where result.ResultType != ScanResultType.NotPerformed
                                     select result).ToArray();

            var total = onlineScanResults.Count() * 100;
            var current = onlineScanResults.Sum(scanResult => scanResult.ProgressPercenage);

            return (int)((double)current / total * 100);
        }

        public event EventHandler<OperationProgressChangedEventArgs> OperationProgressChanged;
        private void OnOperationProgressChanged(OperationProgressChangedEventArgs args)
        {
            EventHandler<OperationProgressChangedEventArgs> temp = OperationProgressChanged;
            if (temp != null)
                temp(this, args);
        }

        public class OperationProgressChangedEventArgs : EventArgs
        {
            public int Percentage { get; set; }
        }

        public FilesStatistics CalculateFilesStatistics()
        {
            return StatisticsCalculator.Calculate(_scanResults.Values);
        }
    }
}
