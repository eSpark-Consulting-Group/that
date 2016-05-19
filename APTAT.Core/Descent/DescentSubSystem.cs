using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Descent.Helpers;
using APTAT.Core.Properties;
using APTAT.Core.Descent.Wmi;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Descent
{

	public class DescentSubSystem: IDescentSubSystem, IDisposable
	{
		private static readonly Guid DescentSubSystemIdentifier = new Guid( @"{6E6F58FD-47AC-4d1e-8063-40C2CA9F332E}" );
		private const string DescentSubSystemName = @"Descent";

        private readonly ConcurrentDictionary<Guid, AptScanOperationContext> _aptScanResultsPerOperation = new ConcurrentDictionary<Guid, AptScanOperationContext>();
        private readonly ConcurrentDictionary<Guid, WmiScanOperationContext> _wmiScanResultsPerOperation = new ConcurrentDictionary<Guid, WmiScanOperationContext>();

        private readonly Dictionary<Guid, DescentResult> _descentResults = new Dictionary<Guid, DescentResult>();


        private IChangeTrackingSubSystem _tracker;
		private DescentMod _descentMod;
		private ISubSystemCatalog _catalog;
		private bool _disposed;
		private Timer _descentResultsCleanupTimer;

		#region IDisposable Members

		public void Dispose()
		{
			Dispose( true );
		}

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_descentMod != null)
                    {
                        _descentMod.Release();
                    }

                    if (_descentResultsCleanupTimer != null)
                    {
                        _descentResultsCleanupTimer.Dispose();
                    }
                }
                _disposed = true;
            }
        }

        #endregion

        

		#region Installation/Uninstallation
        

        private void ProceedDescendOperation(IAptScanOperationContext queryResult, 
            IComputer computer, string login, string password, string scanScope)
		{
            RemoteHostInfo ri = new RemoteHostInfo
            {
                HostName = (computer.IPAddress != null) ? computer.IPAddress.ToString() : computer.DnsName,
                Login = login,
                Password = password,
                TargetUid = computer.Uid
            };
            AptScanResult result;
            queryResult.ScanResults.TryGetValue(computer.Uid, out result);

            var job = new RemoteInstallerJob( queryResult, _tracker, computer);
            _descentMod.StartAptScan( ri, job, computer.Uid, result, queryResult.OperationUid, scanScope );
		}
        #endregion
        private static void CalculateOperationStatistics(IEnumerable<AptScanResult> results, out int failed, out int notPerformed, out int succeeded, out int total, out int canceled)
        {
            notPerformed = 0;
            succeeded = 0;
            failed = 0;
            canceled = 0;

            var scanResults = results as AptScanResult[] ?? results.ToArray();
            total = scanResults.Count();
            var groupped = from result in scanResults group result by result.ResultType into temp select temp;
            foreach (var item in groupped)
            {
                if (item.Key == ScanResultType.NotPerformed)
                    notPerformed = item.Count();
                else if (item.Key == ScanResultType.Failed)
                    failed = item.Count();
                else if (item.Key == ScanResultType.Completed)
                    succeeded = item.Count();
                else if (item.Key == ScanResultType.Canceled)
                    canceled = item.Count();
            }
        }
        
        protected virtual void OnDescentCompleted(DescentEventArgs args)
        {
            EventHandler<DescentEventArgs> temp = DescentCompleted;
            if (temp != null)
                temp(this, args);
        }

        #region IDescentSubSystem Members

        public event EventHandler<DescentEventArgs> DescentCompleted;
		public void StartAptScan( IEnumerable<DeploymentTarget> targets, string login, string password, 
            out Guid operationUid, string scanScope)
        {
            if (login == null)
                throw new ArgumentException("Login name not specified.", "login");
            if (password == null)
                throw new ArgumentException("Password not specified.", "password");
            if (_aptScanResultsPerOperation.Count > 0)
            {
                var item = _aptScanResultsPerOperation.FirstOrDefault();
                if (!item.Value.Completed)
                    throw new AnotherScanIsInProgressException();
            }


		    var computers = GetTargetUidComputerPairs(targets);

		    var onlineTargetUidComputerPairs = (from pair in computers where pair.Computer.IsOnline && !(pair.State is Exception) select pair).ToArray();

            //DescentResult result = new DescentResult(onlineTargetUidComputerPairs);
            var result = new AptScanOperationContext(onlineTargetUidComputerPairs);
            operationUid = result.OperationUid;
			lock( _descentResults )
			{
				_descentResults[result.OperationUid] = result;
			}
            _aptScanResultsPerOperation.Clear();
            _aptScanResultsPerOperation.TryAdd(result.OperationUid, result);


            LogHelper.Trace( TraceEventType.Verbose, TraceCategory.Descent,
				"Apt scan started{0} UID: {1}{0} computers count: {2}{0}",
                Environment.NewLine, operationUid, onlineTargetUidComputerPairs.Count());

            

            #region Descent Operation Completed Delegate
            Action wholeDescentCompletedDelegate = null;
			wholeDescentCompletedDelegate = () =>
			{
                LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent, "DescentCompleted event raised. Operation UID: {0}", result.OperationUid);

                // ===================================================================================================================
                // Deployment finalization
                int failed, notPerformed, succeeded, total, canceled;
                CalculateOperationStatistics(result.ScanResults.Values, out failed, out notPerformed, out succeeded, out total, out canceled);
                result.TotalTargetsCount = total;
                result.SucceededTargetsCount = succeeded;
                result.FailedTargetsCount = failed;
                result.NotPerformedTargetsCount = notPerformed;
                result.CanceledTargetsCount = canceled;

                OnDescentCompleted(new AptScanCompletedEventArgs() { OperationUid = result.OperationUid, Context = result });
                // ===================================================================================================================

                _tracker.UnregisterListener(result);
                result.DescentCompleted -= wholeDescentCompletedDelegate;
			};
			#endregion

			_tracker.RegisterListener( result );
			result.DescentCompleted += wholeDescentCompletedDelegate;

			var descentTargets = computers.Where( item => !( item.State is Exception ) )
                .Select( item => item.Computer ).ToArray();

            //===============================================================================================================================================
            // Add not available (offline) computers into the ScanResults dictionary with "NotPerformed" status - means the scan won't be performed on them.
            // They are required for statistics only
		    result.TotalTargetsCount = descentTargets.Length;
		    List<IComputer> onlineComputers = descentTargets.Where(computer => computer.IsOnline).ToList();

		    if (onlineComputers.Count == 0)
            {
                _tracker.UnregisterListener(result);
                result.DescentCompleted -= wholeDescentCompletedDelegate;
                throw new NoAvailableTargetsException();
            }
		    //===============================================================================================================================================
            
            foreach (IComputer computer in onlineComputers)
            {
                ProceedDescendOperation(result, computer, login, password, scanScope);
            }
        }

	    private static TargetUidComputerPair[] GetTargetUidComputerPairs(IEnumerable<DeploymentTarget> targets)
	    {
	        var deploymentTargets = targets as DeploymentTarget[] ?? targets.ToArray();
	        TargetUidComputerPair[] computers = new TargetUidComputerPair[deploymentTargets.Length];
	        for (int index = 0; index < deploymentTargets.Length; index++)
	        {
	            var target = deploymentTargets[index];
	            IPAddress targetAddress;
	            IPAddress.TryParse(target.IPAddress, out targetAddress);
	            computers[index] = new TargetUidComputerPair(target.Uid, target);
	        }
	        return computers;
	    }

	    public void StartWmiScan(IEnumerable<DeploymentTarget> targets, string login, string password,
	        out Guid operationUid)
	    {
            if (_wmiScanResultsPerOperation.Count > 0)
            {
                var item = _wmiScanResultsPerOperation.FirstOrDefault();
                if (!item.Value.Completed)
                    throw new AnotherScanIsInProgressException();
            }

            // This is done in order not to include offline computers into the DescentResult below. Optimization is needed!
            var targetUidComputerPairs = GetTargetUidComputerPairs(targets);
            var result = new WmiScanOperationContext(targetUidComputerPairs);
            operationUid = result.OperationUid;
            lock (_descentResults)
            {
                _descentResults[result.OperationUid] = result;
            }
            _wmiScanResultsPerOperation.Clear();
            _wmiScanResultsPerOperation.TryAdd(result.OperationUid, result);


            LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
                "Wmi scan started{0} UID: {1}{0} computers count: {2}{0}",
                Environment.NewLine, operationUid, targetUidComputerPairs.Length);



            #region Descent Operation Completed Delegate
            Action wholeDescentCompletedDelegate = null;
            wholeDescentCompletedDelegate = () =>
            {
                LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent, 
                    "WmiScanCompleted event raised. Operation UID: {0}", result.OperationUid);
                OnDescentCompleted(new DescentEventArgs() { OperationUid = result.OperationUid, });

                _tracker.UnregisterListener(result);
                result.DescentCompleted -= wholeDescentCompletedDelegate;
            };
            #endregion

            _tracker.RegisterListener(result);
            result.DescentCompleted += wholeDescentCompletedDelegate;

            var descentTargets = targetUidComputerPairs.Where(item => !(item.State is Exception))
                .Select(item => item.Computer).ToArray();
            WmiScanner wmiScanner = new WmiScanner(_catalog);

            foreach (IComputer computer in descentTargets)
            {
                RemoteHostInfo ri = new RemoteHostInfo
                {
                    HostName = (computer.IPAddress != null) ? computer.IPAddress.ToString() : computer.DnsName,
                    Login = login,
                    Password = password,
                    TargetUid = computer.Uid
                };
                WmiScanResult scanResult;
                result.ScanResults.TryGetValue(computer.Uid, out scanResult);

                var job = new RemoteInstallerJob(result, _tracker, computer);
                wmiScanner.StartWmiScan(ri, job, computer.Uid, scanResult, result.OperationUid);
            }
        }


        public AptScanOperationContext GetScanResultsByOperationUid(Guid operationUid)
        {
            AptScanOperationContext results;
            _aptScanResultsPerOperation.TryGetValue(operationUid, out results);
            return results;
        }
        public AptScanOperationContext GetFirstScanResults()
        {
            var keyValue = _aptScanResultsPerOperation.FirstOrDefault();
            return keyValue.Value;
        }
        public void AbortThreadByTargetUid(Guid uid)
        {
            if (_descentMod == null)
                return;

            _descentMod.AbortThreadByTargetUid(uid);

        }
        public void AbortAllThreads()
        {
            if (_descentMod == null)
                return;

            _descentMod.AbortAllThreads();
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
	    public void Init( ISubSystemCatalog catalog )
		{
            _tracker = catalog.Find<IChangeTrackingSubSystem>();
			_catalog = catalog;
            _descentMod = new DescentMod(_catalog);
            _descentResultsCleanupTimer = new Timer( DescentResultsCleanupTimerCallback, null, Settings.Default.DescentResultLifetime, TimeSpan.FromMilliseconds( Timeout.Infinite ) );
		}

        #endregion

        private void DescentResultsCleanupTimerCallback(object state)
        {
            try
            {
                LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent, "Descent results cleanup running");
                Debug.WriteLine("");

                _descentResultsCleanupTimer.Change(Timeout.Infinite, Timeout.Infinite);
                DateTime utcNow = DateTime.UtcNow;
                List<DescentResult> toBeRemoved = new List<DescentResult>();
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

                _descentResultsCleanupTimer.Change(Settings.Default.DescentResultCleanupPeriod, TimeSpan.FromMilliseconds(Timeout.Infinite));
            }
            catch (ObjectDisposedException)
            {
                // No reaction in case when timer is disposed: it means the subsystem is disposed.
            }
        }
    }

    public class DescentEventArgs : EventArgs
    {
        public Guid OperationUid { get; set; }
    }
}