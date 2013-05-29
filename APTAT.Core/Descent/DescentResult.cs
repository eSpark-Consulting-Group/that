using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CRM.SrvCore.Descent;
using APTAT.Core.Tasks;

namespace APTAT.Core.Descent
{
	public class DescentResult : IDescentQueryResult, IChangeTrackingListener
	{
	//	private readonly ConcurrentDictionary<string, ComputerDescentResult> _resultsByName = new ConcurrentDictionary<string, ComputerDescentResult>();
		private readonly ConcurrentDictionary<Guid, ComputerDescentResult> _resultsByUid = new ConcurrentDictionary<Guid, ComputerDescentResult>();
        private readonly ConcurrentDictionary<Guid, Guid> _descendByIpMappings = new ConcurrentDictionary<Guid, Guid>();
		// Task key => computers by UID. 
		// A task may concern to many target computers (e.g. agent control task - agent upgrade may be managed by a single task).
		private readonly Dictionary<long, Dictionary<Guid, ComputerDescentResult>> _resultsByTask = new Dictionary<long, Dictionary<Guid, ComputerDescentResult>>();
		private readonly Dictionary<Guid, bool> _reportedByUid = new Dictionary<Guid, bool>();
		private readonly bool _withPackages;
		private readonly bool _withUninstall;
		private readonly object _sync = new object();
		private readonly object _actionResultSync = new object();

		private int _inprogressCount;
		private int _succeeded;
		private int _failed;
		private bool _withDelayedComputers;

		public DescentResult( IEnumerable<TargetUidComputerPair> computers, bool withDelayedComputers, InstallPackageInfo package )
		{
			bool withPackages = package != null && package.PackageUids != null && package.PackageUids.Length > 0;
			bool withUninstall = package != null && package.UninstallIncompatible;
			bool update = package != null && package.Update;
			bool foundCis = false;
			foreach( var computer in computers )
			{
				bool compWithPackages = withPackages;
				bool compWithUninstall = withUninstall;
				if( update )
				{
					bool hasCis = computer.Computer.GetCisVersion() != null;
					if( !hasCis )
					{
						compWithPackages = false;
						compWithUninstall = false;
					}
					else
					{
						foundCis = true;
					}
				}
				else
				{
					foundCis = true;
				}

				var result = new ComputerDescentResult( computer.TargetUid, computer.Computer, compWithPackages, compWithUninstall );
				var error = computer.State as Exception;
				if (error != null)
				{
					result.Invalidate(true, false, false, computer.DescentStatus, error.Message, String.Empty);
					_reportedByUid[computer.Computer.Uid] = true;
				}
				else
				{
					_reportedByUid[computer.Computer.Uid] = false;
				}
			//	_resultsByName.AddOrUpdate( computer.Computer.Name, result, ( key, value ) => value );
				_resultsByUid.AddOrUpdate(computer.Computer.Uid, result, (key, value) => value);
			}

			OperationUid = Guid.NewGuid();
			CompletedTime = null;
			_inprogressCount = _resultsByUid.Count(r => !r.Value.Completed);
			_withDelayedComputers = withDelayedComputers && foundCis;
			_withPackages = withPackages && foundCis;
			_withUninstall = withUninstall;
			StartTime = DateTime.UtcNow;
		}

		public ComputerDescentResult[] ComputerResults
		{
			get
			{
				return _resultsByUid.Values.ToArray();
			}
		}

		public Guid OperationUid
		{
			get;
			private set;
		}

		public bool Completed
		{
			get
			{
				return _inprogressCount == 0 && !_withDelayedComputers;
			}
		}

		public DateTime StartTime
		{
			get;
			private set;
		}

		public DateTime? CompletedTime
		{
			get;
			private set;
		}

		public void OnQueryAgentEvent( bool ok, RemoteAgentInformation info, FormattedText errorString )
		{
			throw new NotImplementedException();
		}

		public void OnDescendOperationEvent( bool ok, IComputer computer, FormattedText errorMessage, FormattedText shortStatusMessage, string errorCode, int systemCode, string systemMessage )
		{
            Guid uid = GetComputerOriginalUid(computer);

			OperationDescentResult operationResult;
			ComputerDescentResult computerDescentResult;
			if( !_resultsByUid.TryGetValue( uid, out computerDescentResult ) )
			{
				throw new InvalidOperationException( "Computer result not found by UID " + uid );
			}

			bool withPackages = _withPackages && computerDescentResult.WithPackage;
			if( !ok || !withPackages )
			{
				operationResult = new OperationDescentResult( DescentStage.AgentInstall, true, ok, errorMessage, shortStatusMessage, errorCode, systemMessage, systemCode, 100 );
			}
			else
			{
				operationResult = new OperationDescentResult( DescentStage.AgentInstall, true, ok, FormattedText.Empty, FormattedText.Empty, String.Empty, String.Empty, 0, 0 );
			}

			var inprogressCount = ProcessResult(uid, operationResult);

			LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
				"Computer agent descent {1}{0} operation uid: {2}{0} computer: {3}{0} elapsed: {4}{0} error code: {5} ({6}){0} system message: {7}{0} remaining: {8}{0}",
				Environment.NewLine, ok ? "SUCCEEDED" : "FAILED", OperationUid, computer.Name, DateTime.UtcNow - StartTime, errorCode, systemCode, systemMessage, inprogressCount);

			if( _resultsByUid[uid].AgentCompleted )
			{
				OnAgentDescentCompleted( computer, ok );
			}
		}

		public void OnPackageInstallationEvent(bool ok, IComputer computer, FormattedText statusMessage, FormattedText shortStatusMessage, string errorCode, string systemMessage, int systemCode)
		{
            Guid uid = GetComputerOriginalUid(computer);

			// TODO: Compose package installation error messages.

			var operationResult = new OperationDescentResult( DescentStage.PackageInstall, true, ok, statusMessage, shortStatusMessage, errorCode, systemMessage, systemCode, 100 );
			var inprogressCount = ProcessResult(uid, operationResult);

			LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
				"Computer package descent {1}{0} operation uid: {2}{0} computer: {3}{0} elapsed: {4}{0} error message: {5}{0} remaining: {6}{0}",
				Environment.NewLine, ok ? "SUCCEEDED" : "FAILED", OperationUid, computer.Name, DateTime.UtcNow - StartTime, statusMessage, inprogressCount);
		}

		public void OnIncompatibleUninstallEvent( bool ok, IComputer computer, FormattedText statusMessage, FormattedText shortStatusMessage, string errorCode, string systemMessage, int systemCode, string[] unsupportedIncompatibleProducts, string[] supportedIncompatibleProducts )
		{
            Guid uid = GetComputerOriginalUid(computer);
			OperationDescentResult operationResult;
			if( !ok )
			{
				List<string> extensionTexts = new List<string>();
				if( unsupportedIncompatibleProducts != null && unsupportedIncompatibleProducts.Length > 0 )
				{
					string unsupportedExtensionText = null;
					StringBuilder builder = new StringBuilder();
					foreach( var unsupportedProduct in unsupportedIncompatibleProducts )
					{
						builder.AppendFormat( "  - {0}", unsupportedProduct ).AppendLine();
					}

					unsupportedExtensionText = builder.ToString();
					unsupportedExtensionText = unsupportedExtensionText.TrimEnd( Environment.NewLine.ToCharArray());
					extensionTexts.Add( unsupportedExtensionText );
				}

				if( supportedIncompatibleProducts != null && supportedIncompatibleProducts.Length > 0 )
				{
					string supportedExtensionText = null;
					StringBuilder builder = new StringBuilder();
					foreach( var supportedProduct in supportedIncompatibleProducts )
					{
						builder.AppendFormat( "  - {0}", supportedProduct ).AppendLine();
					}

					supportedExtensionText = builder.ToString();
                    supportedExtensionText = supportedExtensionText.TrimEnd(Environment.NewLine.ToCharArray());
					extensionTexts.Add( supportedExtensionText );
				}

				operationResult = new OperationDescentResult( DescentStage.IncompatibleUninstall, true, ok, statusMessage, shortStatusMessage, errorCode, systemMessage, systemCode, 100, extensionTexts.ToArray(), supportedIncompatibleProducts, null );
			}
			else
			{
				operationResult = new OperationDescentResult( DescentStage.IncompatibleUninstall, true, ok, FormattedText.Empty, FormattedText.Empty, String.Empty, String.Empty, 0, 0, null, supportedIncompatibleProducts, null );
			}

			var inprogressCount = ProcessResult(uid, operationResult);

			LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
				"Incompatible software uninstall {1}{0} operation uid: {2}{0} computer: {3}{0} elapsed: {4}{0} error message: {5}{0} remaining: {6}{0}",
				Environment.NewLine, ok ? "SUCCEEDED" : "FAILED", OperationUid, computer.Name, DateTime.UtcNow - StartTime, statusMessage, inprogressCount);
		}

		public void OnOutdatedCisUninstallEvent( bool ok, IComputer computer, FormattedText statusMessage, FormattedText shortStatusMessage, string errorCode, string systemMessage, int systemCode, string outdatedCis )
		{
            var uid = GetComputerOriginalUid(computer);
			var operationResult = !ok ?
				new OperationDescentResult( DescentStage.OutdatedCisUninstall, true, ok, statusMessage, shortStatusMessage, errorCode, systemMessage, systemCode, 100, null, null, outdatedCis ) :
				new OperationDescentResult( DescentStage.OutdatedCisUninstall, true, ok, FormattedText.Empty, FormattedText.Empty, String.Empty, String.Empty, 0, 0, null, null, outdatedCis );
			var inprogressCount = ProcessResult(uid, operationResult);

			LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
				"Outdated CES uninstall {1}{0} operation uid: {2}{0} computer: {3}{0} elapsed: {4}{0} error message: {5}{0} remaining: {6}{0}",
				Environment.NewLine, ok ? "SUCCEEDED" : "FAILED", OperationUid, computer.Name, DateTime.UtcNow - StartTime, statusMessage, inprogressCount);
		}

        public Guid GetComputerOriginalUid(IComputer computer)
        {
            Guid uid = computer.Uid;
            Guid originalUid = computer.OriginalAgentIdentityOnConnect;

            if (_descendByIpMappings.ContainsKey(uid))
            {
                return _descendByIpMappings[uid];
            }

            // If descending was done using IP, we may have a situation when pc.OriginalAgentIdentityOnConnect
            // differs from pc.UID. It is normal and DescentResult should handle such computers in a special way.
            if (originalUid == uid || originalUid == Guid.Empty)
                return uid; // not our case

            // If we know about OriginalAgentIdentityOnConnect but not about Uid, exchange them.
            if (_resultsByUid.ContainsKey(originalUid) && !_resultsByUid.ContainsKey(uid))
            {
				LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent, "Detected descended by IP computer: {0}. Replacing {1} by {2}", computer.Name, uid, originalUid);

                // Remember this mapping
                _descendByIpMappings.AddOrUpdate(uid, originalUid, (key, value) => value);

                return originalUid;
            }

            return uid;
        }

		public bool ContainsComputer( Guid uid, out ComputerDescentResult result )
		{
            return _resultsByUid.TryGetValue(uid, out result);
			//return _results.TryGetValue( uid, out result );
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
						_succeeded++;
					else
						_failed++;

					_inprogressCount--;
					_reportedByUid[uid] = true;

					CheckCompletion();
				}
				count = _inprogressCount;
			}
			return count;
		}

		private void CheckCompletion()
		{
			if (_inprogressCount == 0 && !_withDelayedComputers && CompletedTime == null)
			{
				CompletedTime = DateTime.UtcNow;
				LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
					"Deployment operation completed{0} uid: {1}{0} elapsed: {2}{0} total: {3}{0} succeeded: {4}{0} failed: {5}{0}",
					Environment.NewLine, OperationUid, CompletedTime.Value - StartTime, _resultsByUid.Count, _succeeded, _failed);
				OnDescentCompleted();
			}
		}
	/*
		internal void AddComputers( IEnumerable<IComputer> computers, out IComputer[] newComputers )
		{
			List<string> dublicatedComputersNames = new List<string>();
			List<IComputer> newComputersList = new List<IComputer>();

#if DEBUG
			int oldCount = _resultsByUid.Count;
#endif

			foreach( var computer in computers )
			{
				var fixedComp = computer;
				_resultsByName.AddOrUpdate( 
					computer.Name,
 					( computerName ) =>
 					{
						var result = new ComputerDescentResult( fixedComp, _withPackages, _withUninstall );
						_resultsByUid.AddOrUpdate( fixedComp.Uid, result, ( uid, res ) => res );
						newComputersList.Add( fixedComp );
						return result;
 					},
					( computerName, existingResult ) => 
					{
						dublicatedComputersNames.Add( existingResult.Computer.Name );
						return existingResult; 
					} );
			}

			newComputers = newComputersList.ToArray();

			lock( _sync )
			{
				_inprogressCount += newComputers.Count();
			}

#if DEBUG
			Debug.WriteLine( String.Format( "Deployment targets added{0} UID: {1}{0} passed computers count: {2}{0} dublicated computers count: {3}{0} old task computers count: {4}{0} new task computers count: {5}{0}",
				Environment.NewLine, OperationUid, computers.Count(), dublicatedComputersNames.Count, oldCount, oldCount + newComputers.Length ), "DSC" );
#endif
		}
	 */
	/*
		internal void FinishAddingDelayedComputers()
		{
			lock( _sync )
			{
				_withDelayedComputers = false;
				CheckCompletion();
			}
		}
	 */
		public void AddPackageTask( ComputerDescentResult result, ITask task )
		{
			lock( _actionResultSync )
			{
				Dictionary<Guid, ComputerDescentResult> taskDescentResults;
				if( !_resultsByTask.TryGetValue( task.Key, out taskDescentResults ) )
				{
					// If a dictionary for task's computers does not exist, then create it.
					taskDescentResults = new Dictionary<Guid, ComputerDescentResult>();
					_resultsByTask[task.Key] = taskDescentResults;
				}

				taskDescentResults[result.Computer.Uid] = result;
			}
		}

		protected virtual void OnAgentDescentCompleted( IComputer computer, bool success )
		{
			Action<IComputer, bool> tmp = AgentDescentCompleted;
			if( tmp != null )
			{
				tmp( computer, success );
			}
		}

		protected virtual void OnDescentCompleted()
		{
			Action tmp = DescentCompleted;
			if( tmp != null )
			{
				tmp();
			}
		}

		public event Action<IComputer, bool> AgentDescentCompleted;

		public event Action DescentCompleted;

		#region IChangeTrackingListener Members

		public void Changed( params ChangesTrackingItem[] items )
		{
			foreach( var item in items )
			{
				if( item.ChangeType == ChangeType.Changed && item.ChangedItem is Tasks.IActionResult )
				{
					Tasks.IActionResult actionResult = ( Tasks.IActionResult )item.ChangedItem;
					lock( _actionResultSync )
					{
						Tasks.ITask task = actionResult.Owner.Owner.Task;
						if( _resultsByTask.ContainsKey( task.Key ) )
						{
							// Extracting task's expected endpoints dictionary.
							Dictionary<Guid, ComputerDescentResult> originalResults = _resultsByTask[task.Key];
							Guid computerUid = actionResult.Owner.Target.Uid;
							ComputerDescentResult originalResult;
							if( originalResults.TryGetValue( computerUid, out originalResult ) )
							{
								// If computer is found by its UID in task's target dictionary then updating computer's descent result.
								Guid uid = GetComputerOriginalUid( originalResult.Computer );
								originalResult = _resultsByUid[uid];

								LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
									"Change tracking reported descent IActionResult changed{0} computer name: {1}{0} computer UID: {2}{0} completion message: {3}{0} completion progress: {4}{0}",
									Environment.NewLine, originalResult.Computer.Name, uid, actionResult.CompletionMessage, actionResult.CompletionProgress );

								ComputerDescentResult newResult = new ComputerDescentResult( originalResult, actionResult.CompletionMessage, actionResult.CompletionMessage, actionResult.CompletionProgress );
								originalResults[computerUid] = newResult;
								_resultsByUid[uid] = newResult;
							}
							else
							{
								// Abnormal situation when computer not found among task's registered targets.
								LogHelper.Trace(TraceEventType.Error, TraceCategory.Descent,
									"Actions's result change processing error. Target computer '{4}' not found by UID {0} among registered task's targets. Action's input: {1}. Registered task's computers: (2): {3}",
									computerUid, actionResult.Action.Input.GetType(), originalResults.Count,
									String.Join( ", ", originalResults.Values.Select( comp => String.Format( "'{0}' ({1})", comp.Computer.Name, comp.Computer.Uid ) ) ), actionResult.Owner.Target.Name );
							}
						}
					}
				}
				else if( item.ChangeType == ChangeType.Changed && item.ChangedItem is IDescentProgressItem )
				{
					IDescentProgressItem progressItem = ( IDescentProgressItem )item.ChangedItem;
                    if (progressItem.OperationUid == OperationUid && _resultsByUid.ContainsKey(progressItem.ComputerUid))
					{
						ComputerDescentResult originalResult = _resultsByUid[progressItem.ComputerUid];
						ComputerDescentResult newResult = new ComputerDescentResult( originalResult, progressItem.StatusMessage, progressItem.StatusMessage, progressItem.ProgressPercentage );
                        Guid uid = GetComputerOriginalUid(originalResult.Computer);

						LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
							"Change tracking reported descent IDescentProgressItem changed{0} computer name: {1}{0} computer UID: {2}{0} completion message: {3}{0} completion progress: {4}{0}",
							Environment.NewLine, originalResult.Computer.Name, uid, progressItem.StatusMessage, progressItem.ProgressPercentage );

						_resultsByUid[progressItem.ComputerUid] = newResult;
					}
				}
			}
		}

		public Guid SessionId
		{
			get
			{
				return OperationUid;
			}
		}

		#endregion
	}
}
