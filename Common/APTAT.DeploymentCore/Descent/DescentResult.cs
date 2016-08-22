using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using APTAT.Common;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Messages;
using APTAT.DeploymentCore.Properties;

namespace APTAT.DeploymentCore.Descent
{
	public class DescentResult : IDescentQueryResult
	{
        private readonly ConcurrentDictionary<Guid, Action<TimeSpan>> _timeoutCallbacks 
            = new ConcurrentDictionary<Guid, Action<TimeSpan>>();
        private readonly IDescentWatcher _descentWatcher;
	    protected readonly DateTime StartTime;

        private readonly Dictionary<Guid, bool> _reportedByUid = new Dictionary<Guid, bool>();

		private readonly object _sync = new object();

		private int _inprogressCount;
		protected int SucceededCount;
        protected int FailedCount;

        public DescentResult(IEnumerable<IComputer> computers, IDevicesSourceDescription devicesSourceDescription)
            :this(computers, devicesSourceDescription, new ComputerDescentResultFactory())
        { }

        protected DescentResult( IEnumerable<IComputer> computers, IDevicesSourceDescription devicesSourceDescription,
            IComputerDescentResultFactory computerDescentResultFactory)
		{
            if (computerDescentResultFactory == null)
                throw new ArgumentNullException(nameof(computerDescentResultFactory));
            _descentWatcher = new DescentWatcher(Settings.Default.AgentDescentingTimeout, OnDescentTimeout);
		    DevicesSourceDescription = devicesSourceDescription;

            foreach ( var computer in computers )
			{
				var result = computerDescentResultFactory.CreateComputerDescentResult(computer);
				_reportedByUid[computer.Uid] = false;
				ResultsByUid.AddOrUpdate(computer.Uid, result, (key, value) => value);
                _descentWatcher.AddIdentity(computer.Uid);
            }

			OperationUid = Guid.NewGuid();
			CompletedTime = null;
			_inprogressCount = ResultsByUid.Count(r => !r.Value.Completed);
		    TotalTargetsCount = ResultsByUid.Count;
			StartTime = DateTime.UtcNow;
            _descentWatcher.Start();
        }

        protected interface IComputerDescentResultFactory
        {
            IComputerDescentResultInternal CreateComputerDescentResult(IComputer computer);
        }

	    protected sealed class ComputerDescentResultFactory : IComputerDescentResultFactory
	    {
	        public IComputerDescentResultInternal CreateComputerDescentResult(IComputer computer)
	        {
	            return new ComputerDescentResult(computer);
	        }

    	}

        protected ConcurrentDictionary<Guid, IComputerDescentResultInternal> ResultsByUid { get; }
            = new ConcurrentDictionary<Guid, IComputerDescentResultInternal>();

        public IDevicesSourceDescription DevicesSourceDescription { get; }

        public int TotalTargetsCount { get; set; }
        public Guid OperationUid { get; }
		public bool Completed { get { return _inprogressCount == 0; } }
		public DateTime? CompletedTime { get; private set; }

		public virtual void OnDescendOperationEvent( bool ok, Guid computerUid, FormattedText errorMessage, FormattedText shortStatusMessage, string errorCode, int systemCode, string systemMessage )
		{
			IOperationDescentResult operationResult;
			IComputerDescentResultInternal computerDescentResult;
			if( !ResultsByUid.TryGetValue(computerUid, out computerDescentResult ) )
			{
				throw new InvalidOperationException( "Computer result not found by UID " + computerUid);
			}
            
			if( !ok)
			{
				operationResult = new OperationDescentResult(false, errorMessage, shortStatusMessage, errorCode, systemMessage, systemCode );
			}
			else
			{
				operationResult = new OperationDescentResult(true, FormattedText.Empty, FormattedText.Empty, String.Empty, String.Empty, 0 );
			}

			var inprogressCount = ProcessResult(computerUid, operationResult);

			LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
				"Computer agent descent {1}{0} operation uid: {2}{0} computer: {3}{0} elapsed: {4}{0} error code: {5} ({6}){0} system message: {7}{0} remaining: {8}{0}",
				Environment.NewLine, ok ? "SUCCEEDED" : "FAILED", OperationUid, computerDescentResult.Computer.Name, DateTime.UtcNow - StartTime, errorCode, systemCode, systemMessage, inprogressCount);

		}
        
		private int ProcessResult(Guid uid, IOperationDescentResult operationResult)
		{
			int count;
			var result = ResultsByUid[uid];
            result.UpdateStatus(operationResult);
			ResultsByUid[uid] = result;

			lock (_sync)
			{
				if (result.Completed && !_reportedByUid[uid])
				{
					if (result.Succeeded)
						SucceededCount++;
					else
						FailedCount++;

					_inprogressCount--;
					_reportedByUid[uid] = true;
                    _descentWatcher.ExcludeIdentity(uid);

                    CheckCompletion();
				}
				count = _inprogressCount;
			}
			return count;
		}

        private void CheckCompletion()
		{
			if (_inprogressCount == 0 && CompletedTime == null)
			{
				CompletedTime = DateTime.UtcNow;
				LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
					"Deployment operation completed{0} uid: {1}{0} elapsed: {2}{0} total: {3}{0} succeeded: {4}{0} failed: {5}{0}",
					Environment.NewLine, OperationUid, CompletedTime.Value - StartTime, ResultsByUid.Count, SucceededCount, FailedCount);
				OnDescentCompleted();
			}
		}

        public void RegisterTimeoutCallback(Guid agentIdentity, Action<TimeSpan> callback)
        {
            _timeoutCallbacks[agentIdentity] = callback;
        }

        public void UnregisterTimeoutCallback(Guid agentIdentity)
        {
            Action<TimeSpan> item;
            _timeoutCallbacks.TryRemove(agentIdentity, out item);
        }
       

        protected virtual void OnDescentTimeout(Guid agentIdentity)
        {
            Action<TimeSpan> onTimeout;
            if (_timeoutCallbacks.TryGetValue(agentIdentity, out onTimeout))
            {
                onTimeout(_descentWatcher.DescentTimeout);
            }
            else
            {
                LogHelper.Trace(TraceEventType.Error, TraceCategory.Descent,
                    "Computer descent result not found{0} computer UID: {1}{0} operation UID: {2}{0}",
                    Environment.NewLine, agentIdentity, OperationUid);
            }
            _descentWatcher.ExcludeIdentity(agentIdentity);
        }

		protected virtual void OnDescentCompleted()
		{
			Action tmp = DescentCompleted;
			if( tmp != null )
			{
				tmp();
			}
		}

		public event Action DescentCompleted;

		#region IChangeTrackingListener Members

		public virtual void Changed( params ChangesTrackingItem[] items )
		{
			foreach( var item in items )
			{
				if( item.ChangeType == ChangeType.Changed && item.ChangedItem is IDescentProgressItem )
				{
					var progressItem = ( IDescentProgressItem )item.ChangedItem;
                    if (progressItem.OperationUid == OperationUid && ResultsByUid.ContainsKey(progressItem.ComputerUid))
					{
						var originalResult = ResultsByUid[progressItem.ComputerUid];
					    if (progressItem.ProgressPercentage != originalResult.Percentage || 
                            originalResult.StatusMessage !=  progressItem.StatusMessage )
					    {

                            originalResult.UpdateStatus(progressItem.StatusMessage
                                , progressItem.ProgressPercentage);
                        }
                        _descentWatcher.NotifyProgress(progressItem.ComputerUid);
                    }
                }
			}
		}


        public List<IComputerDescentResult> ComputerDescentResults
        {
            get
            {
                return this.ResultsByUid.Values.OfType<IComputerDescentResult>().ToList();
            }
        }
        public IComputerDescentResult GetComputerDescentResultByTargetUid(Guid targetUid)
        {
            return ResultsByUid[targetUid];
        }

        public virtual Guid SessionId { get { return OperationUid; } }

		#endregion
	}
}
