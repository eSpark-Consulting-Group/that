using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using APTAT.Common;
using APTAT.DeploymentCore.Descent.Exceptions;
using APTAT.DeploymentCore.Properties;

namespace APTAT.DeploymentCore.Descent
{
    public sealed class DescentSubSystem: IDescentSubSystem
    {
        public DescentSubSystem(IChangeTrackingSubSystem changeTrackingSubSystem,IScheduler timer)
        {
            _tracker = changeTrackingSubSystem;
            _timer = timer;
            _timer.TimeoutCallback += DescentResultsCleanupTimerCallback;

            var period = Settings.Default.DescentResultLifetime.TotalSeconds;
            _timer.SetInterval(TimeSpan.FromSeconds(period));
        }

        private static readonly Guid DescentSubSystemIdentifier = new Guid( @"{6E6F58FD-47AC-4d1e-8063-40C2CA9F332E}" );
		private const string DescentSubSystemName = @"Descent";
                
        private readonly ConcurrentDictionary<Guid, IScanner> _scanners = new ConcurrentDictionary<Guid, IScanner>();

        private readonly Dictionary<Guid, IDescentQueryResult> _descentResults = new Dictionary<Guid, IDescentQueryResult>();

        private readonly IChangeTrackingSubSystem _tracker;
		private bool _disposed;
        private readonly IScheduler _timer = null;

        #region IDisposable Members
        public void Dispose()
		{
			Dispose(true);
		}

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _timer?.Dispose();
            }
            _disposed = true;
        }

        #endregion

        private void OnDescentCompleted(DescentEventArgs args)
        {
            DescentCompleted?.Invoke(this, args);
        }

        #region IDescentSubSystem Members

        public event EventHandler<DescentEventArgs> DescentCompleted;

	    public IDescentQueryResult GetDescentResultsByOperationUid(Guid operationUid)
	    {
	        lock (_descentResults)
	        {
                if (_descentResults.Count == 0) return null;
	            return _descentResults[operationUid];
	        }
	    }

	    public void StartScan(IScanner scanner, out Guid operationUid)
	    {
            var result = scanner.ValidateAndGetResult();
            if (result.TotalTargetsCount == 0)
                throw new NoAvailableTargetsException();
            var operationUidInternal = operationUid = result.OperationUid;
            lock (_descentResults)
            {
                _descentResults[result.OperationUid] = result;
            }
            EventHandler<IDescentResult> wholeDescentCompletedDelegate = null;
            wholeDescentCompletedDelegate = (s,e) =>
            {
                LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
                    "ScanCompleted event raised. Operation UID: {0}", result.OperationUid);
                _tracker.UnregisterListener(result);
                result.DescentCompleted -= wholeDescentCompletedDelegate;
                OnDescentCompleted(scanner.GetDescentCompletedEventArgs(result));
                IScanner sc;
                _scanners.TryRemove(operationUidInternal, out sc);
                sc.Dispose();
            };
            _tracker.RegisterListener(result);
            result.DescentCompleted += wholeDescentCompletedDelegate;
            _scanners.TryAdd(operationUid, scanner);
            scanner.StartScan(_tracker,result);
        }

        public void StopAllScans(int code = 0)
        {
            foreach (var scanner in _scanners)
            {
                scanner.Value.AbortAllThread(code);
            }
        }

        public void StopOperationForTargetUid(Guid[] targetUids)
        {
            foreach (var scanner in _scanners)
            {
                foreach (var target in targetUids)
                    scanner.Value.StopScan(target);
            }
        }

        #endregion

        #region ISubSystem Members

        public string Name
		{
			get { return DescentSubSystemName; }
		}
		public Guid Identifier
		{
			get { return DescentSubSystemIdentifier; }
		}

        #endregion

        private void DescentResultsCleanupTimerCallback(object state, EventArgs e)
        {
            try
            {
                LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent, "Descent results cleanup running");
                Debug.WriteLine("");
        
                DateTime utcNow = DateTime.UtcNow;
                List<IDescentQueryResult> toBeRemoved = new List<IDescentQueryResult>();
                lock (_descentResults)
                {
                    foreach (var descentResult in _descentResults.Values)
                    {
                        if (descentResult.Completed &&
                            descentResult.CompletedTime != null &&
                            utcNow - descentResult.CompletedTime.Value > Settings.Default.DescentResultLifetime)
                        {
                            toBeRemoved.Add(descentResult);
                        }
                    }

                    foreach (var descentResult in toBeRemoved)
                    {
                        _descentResults.Remove(descentResult.OperationUid);

                        LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
                            "Completed descent result removed{0} UID: {1}{0} completed (UTC): {2}{0}",
                            Environment.NewLine, descentResult.OperationUid, descentResult.CompletedTime);
                    }
                }
                _timer.Start(false);
            }
            catch (ObjectDisposedException)
            {
                // No reaction in case when timer is disposed: it means the subsystem is disposed.
            }
        }
    }
}