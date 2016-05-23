using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace APTAT.Core.Descent.Apt
{
    public class AptScanOperationContext : DescentResult, IAptScanOperationContext
    {
        private readonly Guid _sessionId = Guid.NewGuid();
        private readonly ConcurrentDictionary<Guid, AptScanResult> _scanResults;

        public AptScanOperationContext(IEnumerable<TargetUidComputerPair> computers, int totalTargetCount)
            : base(computers)
        {
            _scanResults = new ConcurrentDictionary<Guid, AptScanResult>();
            var scanResultType = ScanResultType.InProgress;
            foreach (var target in computers)
            {
                var computer = target.Computer;
                _scanResults.TryAdd(
                       computer.Uid,
                       new AptScanResult()
                       {
                           HostName = computer.Name,
                           IpAddress = computer.IPAddress != null ? computer.IPAddress.ToString() : String.Empty,
                           ResultType = scanResultType,
                           TargetUid = computer.Uid,
                           OrganizationUnit = computer.OrganizationUnit
                       });

            }
            TotalTargetsCount = totalTargetCount;

        }
        public DateTime Started { get { return StartTime; } }

        private readonly object _sync = new object();

        public ConcurrentDictionary<Guid, AptScanResult> ScanResults
        {
            get { return _scanResults; }
        }

        public override void Changed(params ChangesTrackingItem[] items)
        {
            base.Changed(items);
            if (items == null) return;
            foreach (var item in items)
            {
                if (item.ChangeType == ChangeType.Changed && item.ChangedItem is IDescentProgressItem)
                {
                    DescentProgressItem descentProgressItem = (DescentProgressItem)item.ChangedItem;
                    AptScanResult result;
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

            OnOperationProgressChanged(new AptOperationProgressChangedEventArgs()
            {
                Percentage = CalculateTotalPercentage(),
                ProgressItems = items.Select(c => c.ChangedItem).OfType<DescentProgressItem>().ToArray(),
                OperationUid = OperationUid
            });
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

        public event EventHandler<AptOperationProgressChangedEventArgs> OperationProgressChanged;
        private void OnOperationProgressChanged(AptOperationProgressChangedEventArgs args)
        {
            EventHandler<AptOperationProgressChangedEventArgs> temp = OperationProgressChanged;
            if (temp != null)
                temp(this, args);
        }

        public class AptOperationProgressChangedEventArgs : OperationProgressChangedEventArgs
        {
            public DescentProgressItem[] ProgressItems { get; set; }
        }

        public FilesStatistics CalculateFilesStatistics()
        {
            return StatisticsCalculator.Calculate(_scanResults.Values);
        }
    }
}
