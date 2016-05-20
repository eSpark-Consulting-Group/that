using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using APTAT.Core.Properties;

namespace APTAT.Core.Descent
{
	public class DescentResult : IDescentQueryResult
	{
		private readonly ConcurrentDictionary<Guid, ComputerDescentResult> _resultsByUid = new ConcurrentDictionary<Guid, ComputerDescentResult>();
        private readonly ConcurrentDictionary<Guid, Action<TimeSpan>> _timeoutCallbacks = new ConcurrentDictionary<Guid, Action<TimeSpan>>();
        private readonly IDescentWatcher _descentWatcher;
	    protected readonly DateTime StartTime;

        private readonly Dictionary<Guid, bool> _reportedByUid = new Dictionary<Guid, bool>();

		private readonly object _sync = new object();

		private int _inprogressCount;
		protected int SucceededCount;
        protected int FailedCount;

		public DescentResult( IEnumerable<TargetUidComputerPair> computers)
		{
            _descentWatcher = new DescentWatcher(Settings.Default.AgentDescentingTimeout, OnDescentTimeout);
            
			foreach( var computer in computers )
			{
				var result = new ComputerDescentResult( computer.TargetUid, computer.Computer );
				var error = computer.State as Exception;
				if (error != null)
				{
					result.Invalidate(true, computer.DescentStatus, error.Message, String.Empty);
					_reportedByUid[computer.Computer.Uid] = true;
				}
				else
				{
					_reportedByUid[computer.Computer.Uid] = false;
				}
				_resultsByUid.AddOrUpdate(computer.Computer.Uid, result, (key, value) => value);
                _descentWatcher.AddIdentity(computer.Computer.Uid);
            }

			OperationUid = Guid.NewGuid();
			CompletedTime = null;
			_inprogressCount = _resultsByUid.Count(r => !r.Value.Completed);
		    TotalTargetsCount = _resultsByUid.Count;
			StartTime = DateTime.UtcNow;
            _descentWatcher.Start();
        }


        public int TotalTargetsCount { get; set; }

        public int NotPerformedTargetsCount { get; set; }
        public int SucceededTargetsCount { get; set; }
        public int FailedTargetsCount { get; set; }
        public int CanceledTargetsCount { get; set; }
        public Guid OperationUid
		{
			get;
			private set;
		}

		public bool Completed
		{
			get
			{
				return _inprogressCount == 0;
			}
		}

		

		public DateTime? CompletedTime
		{
			get;
			private set;
		}

		public virtual void OnDescendOperationEvent( bool ok, IComputer computer, FormattedText errorMessage, FormattedText shortStatusMessage, string errorCode, int systemCode, string systemMessage )
		{
            Guid uid = computer.Uid;

			OperationDescentResult operationResult;
			ComputerDescentResult computerDescentResult;
			if( !_resultsByUid.TryGetValue( uid, out computerDescentResult ) )
			{
				throw new InvalidOperationException( "Computer result not found by UID " + uid );
			}
            
			if( !ok)
			{
				operationResult = new OperationDescentResult(false, errorMessage, shortStatusMessage, errorCode, systemMessage, systemCode );
			}
			else
			{
				operationResult = new OperationDescentResult(true, FormattedText.Empty, FormattedText.Empty, String.Empty, String.Empty, 0 );
			}

			var inprogressCount = ProcessResult(uid, operationResult);

			LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
				"Computer agent descent {1}{0} operation uid: {2}{0} computer: {3}{0} elapsed: {4}{0} error code: {5} ({6}){0} system message: {7}{0} remaining: {8}{0}",
				Environment.NewLine, ok ? "SUCCEEDED" : "FAILED", OperationUid, computer.Name, DateTime.UtcNow - StartTime, errorCode, systemCode, systemMessage, inprogressCount);

		}

		
		private int ProcessResult(Guid uid, OperationDescentResult operationResult)
		{
			int count;
			var result = new ComputerDescentResult(_resultsByUid[uid], operationResult);
			_resultsByUid[uid] = result;

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
					Environment.NewLine, OperationUid, CompletedTime.Value - StartTime, _resultsByUid.Count, SucceededCount, FailedCount);
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
					IDescentProgressItem progressItem = ( IDescentProgressItem )item.ChangedItem;
                    if (progressItem.OperationUid == OperationUid && _resultsByUid.ContainsKey(progressItem.ComputerUid))
					{
						ComputerDescentResult originalResult = _resultsByUid[progressItem.ComputerUid];
						ComputerDescentResult newResult = new ComputerDescentResult( originalResult, progressItem.StatusMessage, progressItem.StatusMessage);
                        
						_resultsByUid[progressItem.ComputerUid] = newResult;
                        _descentWatcher.NotifyProgress(progressItem.ComputerUid);
                    }
				}
			}
		}

		public virtual Guid SessionId
		{
			get
			{
				return OperationUid;
			}
		}

		#endregion
	}
}
