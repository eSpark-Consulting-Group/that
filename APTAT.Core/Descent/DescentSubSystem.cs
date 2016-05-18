using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using APTAT.Core.Messages;
using APTAT.Core.Properties;
using APTAT.Core.Descent.Transport;

namespace APTAT.Core.Descent
{

	public class DescentSubSystem: IDescentSubSystem, IDisposable
	{
		private static readonly Guid DescentSubSystemIdentifier = new Guid( @"{6E6F58FD-47AC-4d1e-8063-40C2CA9F332E}" );
		private const string DescentSubSystemName = @"Descent";

        private readonly ConcurrentDictionary<Guid, AptScanOperationContext> _scanResultsPerOperation = new ConcurrentDictionary<Guid, AptScanOperationContext>();
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

        private class RemoteInstallerJob: IDescentCallback//, IDisposable
		{
			private readonly IDescentQueryResult _queryResult;
		    private readonly IChangeTrackingSubSystem _changeTrackingSubSystem;
			private readonly IComputer _computer;
            private Thread _descentThread;
			//private bool _disposed;

			#region RemoteInstallerJob class

			public RemoteInstallerJob(
				IDescentQueryResult queryResult,
                IChangeTrackingSubSystem changeTrackingSubSystem,
				IComputer computer)
			{
				_queryResult = queryResult;
			    _changeTrackingSubSystem = changeTrackingSubSystem;
				_computer = computer;
				IsCompleted = false;
			    _queryResult.RegisterTimeoutCallback(_computer.Uid, OnTimeout);
			}
            public void SetDescentThread()
            {
                if (_descentThread != null)
                {
                    LogHelper.TraceInColor(ConsoleColor.DarkRed, TraceEventType.Warning, TraceCategory.Descent, "Descent thread re-assignment ignored.");
                    return;
                }
                _descentThread = Thread.CurrentThread;
            }
            public bool IsCompleted
			{
				get;
				private set;
			}

		    public IDescentQueryResult QueryResult
		    {
		        get { return _queryResult; }
		    }

            private void OnTimeout(TimeSpan timeout)
            {
                LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent, "Agent descent to computer {0} has exceeded timeout of: {1}", _computer.Name, timeout);

                if (!IsCompleted)
                {
                    try
                    {
                        if (_descentThread != null)
                            _descentThread.Abort(DescentMod.TimeoutAbortString);
                        var responseText = new FormattedText(ServerSideMessagesResources.DescentTimeoutSystemMessage, timeout.ToString());
                        OnDescentCallback(DescentStatus.status_agent_timeout, 0, responseText.ToString());
                    }
                    catch (ThreadStateException ex)
                    {
                        LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent, "{0}: {1}", ex.GetType().Name, ex.Message);
                    }
                }

                _queryResult.UnregisterTimeoutCallback(_computer.Uid);
            }

            public static FormattedText GetDescentStatusInformation( ResourceText errorFormat, DescentStatus status, int win32Error, string errorString, out FormattedText shortMessage )
			{
				try
				{
					FormattedText strMarshal = new FormattedText( ServerSideMessagesResources.ErrorCodeMessageFormat,
						win32Error );

					// Format description
					FormattedText description = GetDescentStatusDescription( status, null,null,null, out shortMessage );

					return new FormattedText( errorFormat,
						description,
						status.ToString(),
						strMarshal,
						errorString
						);
				}
				catch { }

				shortMessage = FormattedText.Empty;
				return FormattedText.Empty;
			}


			public static FormattedText GetDescentStatusDescription( DescentStatus status, string[] extensionTexts,CultureInfo cultureInfo, 
                IMessageTranslatorCollection messageTranslatorCollection, out FormattedText shortMessage )
			{
				ResourceText suggestion;
				ResourceText description;
				// Format description
			    GetDescriptionAndSuggestionByStatus(status, out description, out suggestion);

				shortMessage = new FormattedText( description );

                //string descriptionFormatted;
                //if (!description.TryTranslate(messageTranslatorCollection, cultureInfo, out descriptionFormatted))
                //    descriptionFormatted = description.ToString();

				if( suggestion.Equals( ResourceText.Empty ) )
				{
					if( extensionTexts == null || extensionTexts.Length == 0 )
                        return new FormattedText(ServerSideMessagesResources.DescentFormatDescriptionWithoutSuggestion, description);
					return new FormattedText( description, extensionTexts );
				}
				ResourceText format = ServerSideMessagesResources.DescentStatusDescriptionWithSuggestionFormat;
			    
				if( extensionTexts == null || extensionTexts.Length == 0 )
                    return new FormattedText(format, description, suggestion);
                return new FormattedText(format, description, new FormattedText(suggestion, extensionTexts));
			}

            private static void GetDescriptionAndSuggestionByStatus(DescentStatus status, 
                out ResourceText description, out ResourceText suggestion)
            {
                suggestion = ResourceText.Empty;
                switch (status)
                {
                    case DescentStatus.status_error:
                        description = ServerSideMessagesResources.UnknownErrorMessage;
                        break;
                    case DescentStatus.status_cannot_resolve_remote_hostname:
                        description = ServerSideMessagesResources.CannotResolveHostMessage;
                        suggestion = ServerSideMessagesResources.CannotResolveHostSuggestionMessage;
                        break;
                    case DescentStatus.status_login_problem:
                        description = ServerSideMessagesResources.LoginProblemMessage;
                        suggestion = ServerSideMessagesResources.LoginProblemSuggestionMessage;
                        break;
                    case DescentStatus.status_login_invalid_username_or_bad_password:
                        description = ServerSideMessagesResources.InvalidUserNameOrPasswordMessage;
                        suggestion = ServerSideMessagesResources.LoginProblemSuggestionMessage;
                        break;
                    case DescentStatus.status_login_invalid_username:
                        description = ServerSideMessagesResources.InvalidUserNameMessage;
                        break;
                    case DescentStatus.status_login_trusted_relationship_failure:
                        description = ServerSideMessagesResources.TrustedRelationshipFailureMessage;
                        break;
                    case DescentStatus.status_agent_is_already_installed:
                        description = ServerSideMessagesResources.AgentIsAlreadyInstalledMessage;
                        break;
                    case DescentStatus.status_agent_is_already_installed_and_server_differs:
                        description = ServerSideMessagesResources.AgentIsAlreadyInstalledAndServerDiffersMessage;
                        suggestion = ServerSideMessagesResources.AgentIsAlreadyInstalledAndServerDiffersSuggestionMessage;
                        break;
                    case DescentStatus.status_agent_is_not_installed:
                        description = ServerSideMessagesResources.AgentIsNotInstalledMessage;
                        break;
                    case DescentStatus.status_installer_unknown_product:
                        description = ServerSideMessagesResources.UnknownProductMessage;
                        break;
                    case DescentStatus.status_installer_fatal_error:
                        description = ServerSideMessagesResources.FatalInstallationErrorMessage;
                        break;
                    case DescentStatus.status_installer_install_already_running:
                        description = ServerSideMessagesResources.AnotherInstallationInProgressMessage;
                        suggestion = ServerSideMessagesResources.AnotherInstallationInProgressSuggestionMessage;
                        break;
                    case DescentStatus.status_installer_product_already_installed:
                        description = ServerSideMessagesResources.ProductAlreadyInstalledMessage;
                        break;
                    case DescentStatus.status_uninstall_incompatible_products_not_supported:
                        description = ServerSideMessagesResources.DescentUninstallIncompatibleUnsupportedMessage;
                        suggestion = ServerSideMessagesResources.DescentUninstallIncompatibleUnsupportedSuggestionMessage;
                        break;
                    case DescentStatus.status_uninstall_incompatible_products_partially_not_supported:
                        description = ServerSideMessagesResources.DescentUninstallIncompatibleUnsupportedMessage;
                        suggestion = ServerSideMessagesResources.DescentUninstallIncompatibleUnsupportedSuggestionMessage;
                        break;
                    case DescentStatus.status_host_is_not_accessible_from_remote_pc:
                        description = ServerSideMessagesResources.HostNotAccessibleMessage;
                        suggestion = ServerSideMessagesResources.HostNotAccessibleSuggestionMessage;
                        break;
                    case DescentStatus.status_no_compatible_msi_found:
                        description = ServerSideMessagesResources.NoCompatibleMsiFoundMessage;
                        break;
                    case DescentStatus.status_package_file_hash_is_invalid:
                        description = ServerSideMessagesResources.PackageFileIsCorruptedOrIsNotTheOriginalOneFormat;
                        break;
                    case DescentStatus.status_remote_cannot_copy_file:
                        description = ServerSideMessagesResources.CannotCopyFileMessage;
                        suggestion = ServerSideMessagesResources.CannotCopyFileSuggestionMessage;
                        break;
                    case DescentStatus.status_remote_cannot_setup_descent_service:
                        description = ServerSideMessagesResources.CannotSetupDescentServiceMessage;
                        suggestion = ServerSideMessagesResources.CannotSetupDescentServiceSuggestionMessage;
                        break;
                    case DescentStatus.status_remote_cannot_cleanup_descent_service:
                        description = ServerSideMessagesResources.CannotRemoveRemoteServiceMessage;
                        break;
                    case DescentStatus.status_remote_cannot_run_descent_service:
                        description = ServerSideMessagesResources.CannotStartRemoteServiceMessage;
                        break;
                    case DescentStatus.status_remote_invalid_response:
                        description = ServerSideMessagesResources.InvalidResponseMessage;
                        break;
                    case DescentStatus.status_bad_admin_netpath:
                        description = ServerSideMessagesResources.TargetComputerNotRespondingMessage;
                        suggestion = ServerSideMessagesResources.TargetComputerNotRespondingSuggestionMessage;
                        break;
                    case DescentStatus.status_agent_timeout:
                        description = ServerSideMessagesResources.AgentDescentTimeoutMessage;
                        suggestion = ServerSideMessagesResources.AgentDescentTimeoutSuggestionMessage;
                        break;
                    case DescentStatus.status_package_timeout:
                        description = ServerSideMessagesResources.PackageDescentTimeoutMessage;
                        suggestion = ServerSideMessagesResources.AgentDescentTimeoutSuggestionMessage;
                        break;
                    case DescentStatus.status_managed_target_not_found:
                        description = ServerSideMessagesResources.TargetComputerNotManagedMessage;
                        break;
                    case DescentStatus.status_operation_canceled:
                        description = new ResourceText("null", "Scan operation was canceled.");
                        break;
                    default:
                        description = ServerSideMessagesResources.UnsupportedErrorMessage;
                        break;
                }
            }

			public void OnDescentCallback( DescentStatus status, int win32Error, string response )
			{
				response = ( response ?? String.Empty ).Trim();
				if( IsCompleted )
				{
					LogHelper.Trace( TraceEventType.Verbose, TraceCategory.Descent,
						"Descent installer job is thought to be already completed{0} computer: {1}{0} status: {2}{0} win32Error: {3}{0} response: {4}{0}",
						Environment.NewLine, _computer, status, win32Error, response );
					return;
				}

				IsCompleted = true;
				if( status == DescentStatus.status_success || status == DescentStatus.status_success_agent_updated )
				{
					FormattedText text;
					text = new FormattedText( ServerSideMessagesResources.DescentInstallAgentSuccess );

					if( status == DescentStatus.status_success_agent_updated )
					{
						text = new FormattedText( ServerSideMessagesResources.DescentInstallAgentSuccessUpdated );
					}

					if( QueryResult != null )
					{
                        // TODO: Commented
						//_computer.AgentVersion = _productManagement.AgentVersionProvider.GetCurrentVersionParsed(_computer.SystemKind);
						QueryResult.OnDescendOperationEvent( true, _computer, text, text, status.ToString(), win32Error, response );
                        //_changeTrackingSubSystem.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed, new DescentProgressItem(_computer.Uid, QueryResult.OperationUid, new FormattedText(new ResourceText("null", "Completed")), AptScanStage.OperationCompleted)), false);
					}
				}
                else if(status == DescentStatus.status_operation_canceled)
                {
                    if (QueryResult != null)
                    {
                        FormattedText shortMessage;
                        FormattedText description = GetDescentStatusDescription(status, null, null, null, out shortMessage);
                        _changeTrackingSubSystem.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed, new DescentProgressItem(_computer.Uid, _computer.Name, QueryResult.OperationUid, shortMessage, AptScanStage.OperationCanceled, 100)), false);

                        QueryResult.OnDescendOperationEvent(true, _computer, description, shortMessage, status.ToString(), win32Error, response);
                    }
                }
				else
				{
					// Error occurred
					ResourceText errorFormat = ServerSideMessagesResources.DescentInstallAgentFailedFormat;
					FormattedText messageText = null;
					FormattedText shortMessage = null;
					try
					{
						messageText = GetDescentStatusInformation( errorFormat, status, win32Error, response, out shortMessage );
					}
					catch { }

					if( messageText != null && QueryResult != null )
					{
						FormattedText formattedText;
                        if(status == DescentStatus.status_error)
                            formattedText = new FormattedText(new ResourceText("null", "Failed: {0}"), response);
                        else
                            formattedText = new FormattedText(new ResourceText("null", "Failed: {0}. {1}"), shortMessage, response);
					    _changeTrackingSubSystem.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed, new DescentProgressItem(_computer.Uid, _computer.Name, QueryResult.OperationUid, formattedText, AptScanStage.OperationFailed, 100)), false);

                        QueryResult.OnDescendOperationEvent(false, _computer, messageText, shortMessage, status.ToString(), win32Error, response);
					}
				}
			}

			#endregion

		}

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
            ScanResult result;
            queryResult.ScanResults.TryGetValue(computer.Uid, out result);

            var job = new RemoteInstallerJob( queryResult, _tracker, computer);
            _descentMod.StartAptScan( ri, job, computer.Uid, result, queryResult.OperationUid, scanScope );
		}
        #endregion
        private static void CalculateOperationStatistics(IEnumerable<ScanResult> results, out int failed, out int notPerformed, out int succeeded, out int total, out int canceled)
        {
            notPerformed = 0;
            succeeded = 0;
            failed = 0;
            canceled = 0;

            var scanResults = results as ScanResult[] ?? results.ToArray();
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

        protected internal virtual void DoDescendOperation(IAptScanOperationContext queryResult,
			IComputer[] computers, string login, string password, string scanScope)
		{
			if( login == null )
				throw new ArgumentException( "Login name not specified.", "login" );
			if( password == null )
				throw new ArgumentException( "Password not specified.", "password" );
            
            foreach (IComputer computer in computers)
            {
                ProceedDescendOperation(queryResult, computer, login, password, scanScope);
            }
        }
        protected virtual void OnDescentCompleted(DescentCompletedEventArgs args)
        {
            EventHandler<DescentCompletedEventArgs> temp = DescentCompleted;
            if (temp != null)
                temp(this, args);
        }

        #region IDescentSubSystem Members

        public event EventHandler<DescentCompletedEventArgs> DescentCompleted;
		public void StartAptScan( IEnumerable<TargetUidComputerPair> computers, string login, string password, 
            out Guid operationUid, string scanScope)
		{
            if (_scanResultsPerOperation.Count > 0)
            {
                var item = _scanResultsPerOperation.FirstOrDefault();
                if (!item.Value.Completed)
                    throw new AnotherScanIsInProgressException();
            }

            // This is done in order not to include offline computers into the DescentResult below. Optimization is needed!
            var targetUidComputerPairs = computers as TargetUidComputerPair[] ?? computers.ToArray();
		    var onlineTargetUidComputerPairs = (from pair in targetUidComputerPairs where pair.Computer.IsOnline && !(pair.State is Exception) select pair).ToArray();

            //DescentResult result = new DescentResult(onlineTargetUidComputerPairs);
            var result = new AptScanOperationContext(onlineTargetUidComputerPairs);
            operationUid = result.OperationUid;
			lock( _descentResults )
			{
				_descentResults[result.OperationUid] = result;
			}
            _scanResultsPerOperation.Clear();
            _scanResultsPerOperation.TryAdd(result.OperationUid, result);


            LogHelper.Trace( TraceEventType.Verbose, TraceCategory.Descent,
				"Deployment operation started{0} UID: {1}{0} computers count: {2}{0}",
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

                OnDescentCompleted(new DescentCompletedEventArgs() { OperationUid = result.OperationUid, Context = result });
                // ===================================================================================================================

                _tracker.UnregisterListener(result);
                result.DescentCompleted -= wholeDescentCompletedDelegate;
			};
			#endregion

			_tracker.RegisterListener( result );
			result.DescentCompleted += wholeDescentCompletedDelegate;

			var descentTargets = targetUidComputerPairs.Where( item => !( item.State is Exception ) )
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

            DoDescendOperation(result, onlineComputers.ToArray(), login, password, scanScope);
		}

	    public void StartWmiScan(IEnumerable<TargetUidComputerPair> computers, string login, string password,
	        out Guid operationUid)
	    {
	        throw new NotImplementedException();
	    }


        public AptScanOperationContext GetScanResultsByOperationUid(Guid operationUid)
        {
            AptScanOperationContext results;
            _scanResultsPerOperation.TryGetValue(operationUid, out results);
            return results;
        }
        public AptScanOperationContext GetFirstScanResults()
        {
            var keyValue = _scanResultsPerOperation.FirstOrDefault();
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

    public class DescentCompletedEventArgs : EventArgs
    {
        public Guid OperationUid { get; set; }
        public AptScanOperationContext Context { get; set; }
    }
}