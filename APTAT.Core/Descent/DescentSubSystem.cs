using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Threading;
using APTAT.Core;
using APTAT.Core.Messages;
using APTAT.Core.Properties;
using CRM.SrvCore.Descent;
using APTAT.Core.Descent.Transport;
using APTAT.Core.Discovery;
using APTAT.Core.Storage;
using APTAT.Core.Tasks;

namespace APTAT.Core.Descent
{
    public class AptScanOperationContext : IChangeTrackingListener
    {
        private Guid _sessionId = Guid.NewGuid();
        private readonly ConcurrentDictionary<Guid, ScanResult> _scanResults;

        public AptScanOperationContext()
        {
            _scanResults = new ConcurrentDictionary<Guid, ScanResult>();
        }

        public Guid OperationUid { get; set; }
        public string TargetsSourceName { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }

        public int TotalTargetsCount { get; set; }
        public int NotPerformedTargetsCount { get; set; }
        public int SucceededTargetsCount { get; set; }
        public int FailedTargetsCount { get; set; }
        public int CanceledTargetsCount { get; set; }

        public ConcurrentDictionary<Guid, ScanResult> ScanResults
        {
            get { return _scanResults; }
        }

        public void Changed(params ChangesTrackingItem[] items)
        {
            foreach (var item in items)
            {
                if(item.ChangeType == ChangeType.Changed && item.ChangedItem is IDescentProgressItem)
                {
                    DescentProgressItem descentProgressItem = (DescentProgressItem) item.ChangedItem;
                    _scanResults[descentProgressItem.ComputerUid].ProgressPercenage = descentProgressItem.ProgressPercentage;
                }
            }

            OnOperationProgressChanged(new OperationProgressChangedEventArgs(){Percentage = CalculateTotalPercentage()});
        }

        public Guid SessionId
        {
            get { return _sessionId; }
        }

        private int CalculateTotalPercentage()
        {
            var onlineScanResults = from result in _scanResults.Values
                                    where result.ResultType != ScanResultType.NotPerformed
                                    select result;

            int total = onlineScanResults.Count() * 100;
            int current = 0;
            foreach (var scanResult in onlineScanResults)
            {
                current += scanResult.ProgressPercenage;
            }

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
    }

    public class MarshalByRefNullLifetime : MarshalByRefObject
    {
        #region Overriden MarshalByRefObject's members

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>
        /// null - object's lifetime is indefinite
        /// </returns>
        /// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
        /// <PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure"/></PermissionSet>
        [SecurityCritical]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion
    }

	public class DescentSubSystem: MarshalByRefNullLifetime, IDescentSubSystem, IDisposable
	{
		private static readonly Guid DescentSubSystemIdentifier = new Guid( @"{6E6F58FD-47AC-4d1e-8063-40C2CA9F332E}" );
		private const string DescentSubSystemName = @"Descent";

		private readonly Dictionary<Guid, DescentResult> _descentResults = new Dictionary<Guid, DescentResult>();
		private IDirectoryResolver _resolver;
		private IMessagesListener _messages;
		private IMessagingSubSystem _messaging;
		private IDirectorySubSystem _directory;
		private IDiscoverySubSystem _discovery;
		private ITasksSubSystem _tasks;
		private IChangeTrackingSubSystem _tracker;
		private IProductManagementSubSystem _productManagement;
		private IEntityStorage _dal;
		private DescentMod _descentMod;
		private ISubSystemCatalog _catalog;
		private bool _disposed;
		private Timer _descentResultsCleanupTimer;
		private WindowsAgentPackageHelper _windowsPackageHelper;
		private DebianAgentPackageHelper _debianPackageHelper;
		private MacOSAgentPackageHelper _macosPackageHelper;

	    public DescentSubSystem()
	    {
	        
	    }

		public struct AgentInstallationResult
		{
			private bool _ok;
			private RemoteAgentInformation _remoteAgentInfo;
			private FormattedText _error;
			public AgentInstallationResult( bool ok, RemoteAgentInformation remoteAgentInfo, FormattedText error )
			{
				_ok = ok;
				_remoteAgentInfo = remoteAgentInfo;
				_error = error;
			}
			public bool IsOk { get { return _ok; } }
			public RemoteAgentInformation RemoteAgentInfo { get { return _remoteAgentInfo; } }
			public FormattedText Error { get { return _error; } }
		}

		public IDescentWatcherFactory DescentWatcherFactory
		{
			get;
			set;
		}

		private void Dispose( bool disposing )
		{
			if( !_disposed )
			{
				if( disposing )
				{
					if( _descentMod != null )
					{
						_descentMod.Release();
					}

					if( _descentResultsCleanupTimer != null )
					{
						_descentResultsCleanupTimer.Dispose();
					}
				}
				_disposed = true;
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose( true );
		}

		#endregion

		private class RemoteInstallerJob: IDescentCallback//, IDisposable
		{
			private readonly IDescentQueryResult _queryResult;
			private readonly IProductManagementSubSystem _productManagement;
		    private readonly IChangeTrackingSubSystem _changeTrackingSubSystem;
			private readonly IComputer _computer;
			private readonly bool _isUndescent;
			//private bool _disposed;

			#region RemoteInstallerJob class

			public RemoteInstallerJob(
				IDescentQueryResult queryResult,
				IProductManagementSubSystem productManagement,
                IChangeTrackingSubSystem changeTrackingSubSystem,
				IComputer computer,
				bool isUndescent )
			{
				_queryResult = queryResult;
				_productManagement = productManagement;
			    _changeTrackingSubSystem = changeTrackingSubSystem;
				_computer = computer;
				_isUndescent = isUndescent;
				IsCompleted = false;
				//_owner._installerJobs.Add( this );
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

		    public IChangeTrackingSubSystem ChangeTrackingSubSystem
		    {
		        get { return _changeTrackingSubSystem; }
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

            private static void GetDescriptionAndSuggestionByStatus(DescentStatus status, out ResourceText description, out ResourceText suggestion)
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
					if( _isUndescent )
					{
						text = new FormattedText( ServerSideMessagesResources.DescentUninstallAgentSuccess );
					}
					else
					{
						text = new FormattedText( ServerSideMessagesResources.DescentInstallAgentSuccess );
					}

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
					ResourceText errorFormat = _isUndescent ?
									ServerSideMessagesResources.DescentUninstallAgentFailedFormat :
									ServerSideMessagesResources.DescentInstallAgentFailedFormat;
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
        // TODO: Commented
        //private void ControlAgentsByTask( ActionFactoryAgentControl.DataInput.ControlAction controlAction,
        //    IDescentQueryResult queryResult, IComputer[] computers, bool skipNotificationsForThisSession )
        //{
        //    ResourceText[] notificationTexts = new ResourceText[2];

        //    TaskSubSystem taskSubsystem = _catalog.Find<TaskSubSystem>();
        //    ISequence sequence = new Sequence();
        //    IActionFactory factory = new ActionFactoryAgentControl();
        //    Action action = new Action( factory );
        //    ActionFactoryAgentControl.DataInput input = ( ActionFactoryAgentControl.DataInput )action.Input;

        //    switch( controlAction )
        //    {
        //        case ActionFactoryAgentControl.DataInput.ControlAction.Uninstall:
        //        {
        //            input.AgentControlAction = ActionFactoryAgentControl.DataInput.ControlAction.Uninstall;
        //            notificationTexts[0] = ServerSideMessagesResources.AgentUninstallationSucceededMessage;
        //            notificationTexts[1] = ServerSideMessagesResources.AgentUninstallationFailedMessage;
        //            break;
        //        }
        //        case ActionFactoryAgentControl.DataInput.ControlAction.Update:
        //        {
        //            input.AgentControlAction = ActionFactoryAgentControl.DataInput.ControlAction.Update;
        //            // Reinstall by task is not needed - there are other ways to have Agent updated (auto-update or delete/re-deploy).
        //            // input.ForceUpdate = true; // to make reinstall possible
        //            notificationTexts[0] = ServerSideMessagesResources.AgentIsUptodateMessage;
        //            notificationTexts[1] = ServerSideMessagesResources.AgentUpdateFailedMessage;
        //            break;
        //        }
        //        default:
        //            throw new ArgumentException();
        //    }

        //    sequence.AddAction( action );
        //    sequence.IsInternal = true;

        //    TaskSubSystem.TaskCompletedDelegate onAgentControlTaskCompleted = taskResult =>
        //    {
        //        foreach (SequenceResult sequenceResult in taskResult.SequenceResults)
        //        {
        //            IComputer pc = sequenceResult.Target;
        //            bool ok = (sequenceResult.Status == ResultStatus.Succeeded);

        //            // send notification
        //            FormattedText notificationText = new FormattedText(ok ? notificationTexts[0] : notificationTexts[1]);
        //            _messages.InjectServerSideNotification(pc, ok ? NotificationLevel.Information : NotificationLevel.Error,
        //                notificationText, skipNotificationsForThisSession);

        //            // send callback
        //            if (queryResult != null)
        //            {
        //                var message = ok ? sequenceResult.ActionResults[0].CompletionMessage : sequenceResult.ActionResults[0].ErrorMessage;
        //                if (String.IsNullOrEmpty(message.ToString()))
        //                    message = notificationText;

        //                queryResult.OnDescendOperationEvent(ok, sequenceResult.Target, message, message, String.Empty, 0, String.Empty);
        //            }

        //        }
        //    };

        //    using (_dal.ContextScope())
        //    {
        //        // Need to run task even if no targets exist
        //        foreach (var computer in computers.DefaultIfEmpty())
        //        {
        //            Task task = null;
        //            switch (controlAction)
        //            {
        //                case ActionFactoryAgentControl.DataInput.ControlAction.Uninstall:
        //                    task = new Task(DescentResources.UninstallAgentTaskName, DescentResources.UninstallAgentTaskDescription);
        //                    break;
        //                case ActionFactoryAgentControl.DataInput.ControlAction.Update:
        //                    task = new Task(DescentResources.UpdateAgentTaskName, DescentResources.UpdateAgentTaskDescription);
        //                    break;
        //                default:
        //                    throw new ArgumentException();
        //            }
        //            task.Sequence = sequence;
        //            task.IsInternal = true;
        //            task.Visible = true;
        //            task.Target = new TaskTargetComputers(computer != null ? new[] { computer } : computers);

        //            // Save task and its sequence to get their actual DB keys.
        //            // This is needed before task adding 
        //            // to IDescentQueryResult's accepted tasks
        //            // in order to track actions progress messages.
        //            _dal.SaveObject(sequence);
        //            _dal.SaveObject(task);

        //            if (queryResult != null && computer != null)
        //            {
        //                ComputerDescentResult cdr;
        //                if (queryResult.ContainsComputer(computer.Uid, out cdr))
        //                    queryResult.AddPackageTask(cdr, task);
        //            }

        //            taskSubsystem.ExecuteTask(task, onAgentControlTaskCompleted);
        //        }
        //    }
        //}

        private void ProceedDescendOperation(bool descend, IDescentQueryResult queryResult, IComputer computer, string login, string password, bool skipNotificationsForThisSession, string scanScope)
		{
			RemoteHostInfo ri = new RemoteHostInfo();
			ri.HostName = ( computer.IPAddress != null ) ? computer.IPAddress.ToString() : computer.DnsName;
			ri.Login = login;
			ri.Password = password;
		    ri.TargetUid = computer.Uid;

			var job = new RemoteInstallerJob( queryResult, _productManagement, _tracker, computer, !descend );

			if( descend )
				_descentMod.InstallAgent( ri, job, computer.Uid, GetSocketPortForAgent(), queryResult.OperationUid, scanScope );
			else
				_descentMod.UninstallAgent( ri, job );
		}

		private void ControlAgents( ActionFactoryAgentControl.DataInput.ControlAction controlAction, IDescentQueryResult queryResult,
			IComputer[] computers, string login, string password, bool skipNotificationsForThisSession, string scanScope )
		{
			// Find out which computers support agent control action and which don't
			List<IComputer> computersByTask = new List<IComputer>();
			List<IComputer> computersByDescent = new List<IComputer>();
			bool descend;

			switch( controlAction )
			{
				case ActionFactoryAgentControl.DataInput.ControlAction.Uninstall:
					descend = false;
					break;
				case ActionFactoryAgentControl.DataInput.ControlAction.Update:
					descend = true;
					break;
				default:
					throw new ArgumentException();
			}

			foreach( IComputer pc in computers )
			{
				bool passwordRequired = !TaskVersionValidator.CheckAgentSupportsOperation(
					new IComputer[] { pc }, TaskVersionValidator.AgentOperation.ActionControlAgent, false );

				if( passwordRequired )
				{
					computersByDescent.Add( pc );
				}
				else
				{
					computersByTask.Add( pc );
				}
			}

			foreach( IComputer computer in computersByDescent )
			{
				ProceedDescendOperation( descend, queryResult, computer, login, password, skipNotificationsForThisSession, scanScope );
			}

			// Yes, computersByTask may be empty here - should execute anyway.
            // TODO: Commented
			//ControlAgentsByTask( controlAction, queryResult, computersByTask.ToArray(), skipNotificationsForThisSession );
		}
		#endregion

		protected internal virtual void DoDescendOperation( IDescentQueryResult queryResult, bool descend,
			IComputer[] computers, string login, string password, bool skipNotificationsForThisSession, string scanScope )
		{
			if( login == null )
				throw new ArgumentException( "Login name not specified.", "login" );
			if( password == null )
				throw new ArgumentException( "Password not specified.", "password" );

			ControlAgents( descend ? ActionFactoryAgentControl.DataInput.ControlAction.Update : ActionFactoryAgentControl.DataInput.ControlAction.Uninstall,
				queryResult, computers, login, password, skipNotificationsForThisSession, scanScope );
		}

		public IAgentPackageHelper GetAgentPackageHelper( SystemKind systemKind )
		{
			if( systemKind.HasFlag( SystemKind.Linux ) )
			{
				return _debianPackageHelper;
			}

			if( systemKind.HasFlag( SystemKind.MacOS ) )
			{
				return _macosPackageHelper;
			}

			return _windowsPackageHelper;
		}

		public IAgentPackageHelper GetAgentPackageHelper(Guid pluginUid)
		{
			if (pluginUid == PluginGuids.LinuxDebianAgentGuid)
				return _debianPackageHelper;
			if (pluginUid == PluginGuids.MacOSXAgentGuid)
				return _macosPackageHelper;
			if (pluginUid == PluginGuids.WindowsAgentGuid)
				return _windowsPackageHelper;

			return null;
		}

		public IAgentPackageHelper[] GetAgentPackageHelpers()
		{
			return new IAgentPackageHelper[]
			{
				_debianPackageHelper,
				_macosPackageHelper,
				_windowsPackageHelper,
			};
		}

		#region IDescentSubSystem Members

		public byte[] GetServerDsaPublicKey()
		{
			byte[] publicKey, privateKey;
			DsaKeysLoader.GetDsaKeys( out privateKey, out publicKey );
			return publicKey;
		}
	
		public bool CreateAgentInstallers()
		{
			bool result = true;
			
            // TODO: Commented
            //using (var context = _dal.Storage.GetContext())
            //{
            //    foreach (var agentHelper in GetAgentPackageHelpers())
            //    {
            //        var agentProduct = context.ManagedProductRepository.GetAgentProduct(agentHelper.AgentKind);
            //        result &= agentHelper.GenerateInstaller(agentProduct);
            //    }
            //}

			return result;
		}

		public ushort GetSocketPortForAgent()
		{
			return ( ushort )Settings.Default.AgentSocketPort;
		}

		private class QueryAgentThreadParam
		{
			public IComputer _computer;
			public string _login;
			public string _password;
			public IDescentQueryResult _queryResult;

			public QueryAgentThreadParam( IComputer computer, string login, string password, IDescentQueryResult queryResult )
			{
				_computer = computer;
				_login = login;
				_password = password;
				_queryResult = queryResult;
			}
		};

        // TODO: Commented
        //private void QueryAgentInformationImpl( QueryAgentThreadParam param )
        //{
        //    IComputer c = param._computer;

        //    string dnsName = c.DnsName;
        //    RemoteAgentInformation info = new RemoteAgentInformation();
        //    info.Computer = c;

        //    //DescentMessageCallbackHandler handler = new DescentMessageCallbackHandler( this, c, info );

        //    RemoteHostInfo ri = new RemoteHostInfo();
        //    ri._hostName = dnsName;
        //    ri._login = param._login;
        //    ri._password = param._password;

        //    AgentInformation agentInformation;

        //    try
        //    {
        //        agentInformation = _descentMod.QueryAgentInformation( ri, GetSocketPortForAgent() );
        //    }
        //    catch( DescentException ex )
        //    {
        //        try
        //        {
        //            ResourceText errorFormat = ServerSideMessagesResources.DescentQueryAgentInfoFailedFormat;
        //            FormattedText shortMessage;
        //            FormattedText strMessage = RemoteInstallerJob.GetDescentStatusInformation( errorFormat, ex._status, ex._win32error, ex._errorString, out shortMessage );
        //            //{
        //            //    AgentInstallationResult a = new AgentInstallationResult( true, info, strMessage );
        //            //    AgentInstallationResults.Add( a );
        //            //}

        //            param._queryResult.OnQueryAgentEvent( false, info, strMessage );
        //        }
        //        catch( Exception ex1 )
        //        {
        //            LogHelper.Trace( TraceEventType.Error, TraceCategory.Descent, ex1.Message );
        //        }
        //        return;
        //    }

        //    Version latestAgentVersion = _productManagement.AgentVersionProvider.GetCurrentVersionParsed(c.SystemKind);
        //    ConvertAgentInformationToRemoteHostInfo( c, info, agentInformation, latestAgentVersion );

        //    try
        //    {
        //        //{
        //        //    AgentInstallationResult a = new AgentInstallationResult( true, info, FormattedText.Empty );
        //        //    AgentInstallationResults.Add( a );
        //        //}
        //        param._queryResult.OnQueryAgentEvent( true, info, FormattedText.Empty );
        //    }
        //    catch( Exception ex )
        //    {
        //        LogHelper.Trace( TraceEventType.Error, TraceCategory.Descent, ex.Message );
        //    }
        //}

        // TODO: Commented
        //private AgentInstallationResult GetAgentInformation( QueryAgentThreadParam param )
        //{
        //    AgentInstallationResult result;
        //    IComputer c = param._computer;

        //    string dnsName = c.DnsName;
        //    RemoteAgentInformation info = new RemoteAgentInformation();
        //    info.Computer = c;

        //    //DescentMessageCallbackHandler handler = new DescentMessageCallbackHandler( this, c, info );

        //    RemoteHostInfo ri = new RemoteHostInfo();
        //    ri._hostName = dnsName;
        //    ri._login = param._login;
        //    ri._password = param._password;

        //    AgentInformation agentInformation;

        //    try
        //    {
        //        agentInformation = _descentMod.QueryAgentInformation( ri, GetSocketPortForAgent() );
        //    }
        //    catch( DescentException ex )
        //    {
        //        ResourceText errorFormat = ServerSideMessagesResources.DescentQueryAgentInfoFailedFormat;
        //        FormattedText shortMessage;
        //        FormattedText strMessage = RemoteInstallerJob.GetDescentStatusInformation(
        //            errorFormat, ex._status, ex._win32error, ex._errorString, out shortMessage );
        //        result = new AgentInstallationResult( true, info, strMessage );
        //        return result;
        //    }

        //    Version latestAgentVersion = _productManagement.AgentVersionProvider.GetCurrentVersionParsed(c.SystemKind);
        //    ConvertAgentInformationToRemoteHostInfo( c, info, agentInformation, latestAgentVersion );
        //    result = new AgentInstallationResult( true, info, FormattedText.Empty );

        //    return result;
        //}

        // TODO: Commented
        //private static void ConvertAgentInformationToRemoteHostInfo( IComputer c, RemoteAgentInformation info, AgentInformation agentInformation, Version latestAgentVersion )
        //{
        //    info.Installed = agentInformation.Installed;
        //    info.ServerHostName = agentInformation.ServerHostName;
        //    info.ServerPort = agentInformation.ServerPort;
        //    info.VersionMajor = agentInformation.VersionMajor;
        //    info.VersionMinor = agentInformation.VersionMinor;
        //    info.CurrentServerAccessible = agentInformation.CurrentServerAccessible;

        //    if( !info.CurrentServerAccessible )
        //    {
        //        info.ErrorDetails = string.Format(
        //            DescentResources.CesmServerInNotReachableMessage,
        //            agentInformation.ServerHostName );
        //    }

        //    Version agentVersion = new Version( ( int )info.VersionMajor, ( int )info.VersionMinor );
        //    c.AgentVersion = agentVersion;
        //    info.Outdated = ( latestAgentVersion > agentVersion );
        //}

        // TODO: Commented
        //private void QueryAgentInformationThread( object obj )
        //{
        //    QueryAgentThreadParam param = ( QueryAgentThreadParam )obj;
        //    try
        //    {
        //        QueryAgentInformationImpl( param );
        //    }
        //    catch( Exception ex )
        //    {
        //        LogHelper.Trace( TraceEventType.Error, TraceCategory.Descent, ex.ToString() );
        //    }
        //}

        // TODO: Commented
        //public void QueryAgentInformation( IDescentQueryResult queryCallback,
        //    IDirectoryItem[] items, string login, string password )
        //{
        //    List<Thread> threads = new List<Thread>();
        //    foreach( IDirectoryItem item in items )
        //    {
        //        IComputer c = item as IComputer;
        //        if( c == null )
        //            continue;

        //        if( c.IsLicensed == false )
        //            continue;

        //        QueryAgentThreadParam param = new QueryAgentThreadParam( c, login, password, queryCallback );
        //        Thread oThread = new Thread( QueryAgentInformationThread );
        //        oThread.SetApartmentState( ApartmentState.MTA );
        //        oThread.Start( param );
        //        threads.Add( oThread );
        //    }

        //    // wait for finishing
        //    foreach( Thread th in threads )
        //    {
        //        th.Join();
        //    }
        //}

        // TODO: Commented
        //public void CollectEligibleComputersForDescendOperation( bool descend, IDirectoryItem[] items, ref List<IComputer> eligibleComputers )
        //{
        //    foreach( IDirectoryItem currentDirItem in items )
        //    {
        //        if( null == currentDirItem )
        //        {
        //            throw new ArgumentNullException( "items" );
        //        }
        //        IDirectoryItemLookup lookup = ( IDirectoryItemLookup )_resolver;
        //        IDirectoryItem item = lookup.Lookup( currentDirItem.Key );
        //        if( null == item )
        //        {
        //            throw new KeyNotFoundException();
        //        }


        //        if( item.Visible == false )
        //            continue;

        //        if( item is IFolder )
        //        {
        //            List<IDirectoryItem> subItems = new List<IDirectoryItem>();
        //            foreach( IDirectoryItem directoryItem in ( ( IFolder )item ).Items )
        //            {
        //                subItems.Add( directoryItem );
        //            }

        //            CollectEligibleComputersForDescendOperation( descend, subItems.ToArray(), ref eligibleComputers );
        //            continue;
        //        }

        //        IComputer computer = item as IComputer;
        //        if( computer == null )
        //            continue;

        //        if( !computer.IsLicensed )
        //        {
        //            FormattedText text;
        //            if( descend )
        //            {
        //                text = new FormattedText(
        //                    ServerSideMessagesResources.ComputerShoudBeManagedToPerformAgentInstallationMessage );
        //            }
        //            else
        //            {
        //                text = new FormattedText(
        //                    ServerSideMessagesResources.ComputerShoudBeManagedToPerformAgentUninstallationMessage );
        //            }

        //            _messages.InjectServerSideNotification( computer, NotificationLevel.Error, text );
        //            continue;
        //        }
        //        eligibleComputers.Add( computer );
        //    }
        //}

        // TODO: Commented
        //public void DoDescendOperationOnFolders( IDescentQueryResult queryCallback, bool descend,
        //    IDirectoryItem[] items, string login, string password, bool skipNotificationsForThisSession )
        //{
        //    List<IComputer> eligibleComputers = new List<IComputer>();
        //    //AgentInstallationResults.Clear();
        //    CollectEligibleComputersForDescendOperation( descend, items, ref eligibleComputers );
        //    DoDescendOperation( queryCallback, descend, eligibleComputers.ToArray(), login, password, skipNotificationsForThisSession );
        //}

        // TODO: Commented
        //public void UpdateAgentByTask(IComputer computer)
        //{
        //    ControlAgentsByTask(ActionFactoryAgentControl.DataInput.ControlAction.Update, null, new[] { computer }, false);
        //}

        // TODO: Commented
        //public void UnDescentOnGroup( IPersistentGroup group, string login, string password )
        //{
        //    if( null == group )
        //        throw new ArgumentNullException( @"group" );

        //    var exGroup = _directory.GetGroup( group.Key );
        //    if( exGroup != null )
        //    {
        //        DoDescendOperationOnFolders( null, false, exGroup.Computers.ToArray(), login, password, false );
        //    }
        //}

        private readonly ConcurrentDictionary<Guid, AptScanOperationContext> _scanResultsPerOperation = new ConcurrentDictionary<Guid, AptScanOperationContext>();
        public event EventHandler<DescentCompletedEventArgs> DescentCompleted;
        protected virtual void OnDescentCompleted(DescentCompletedEventArgs args)
        {
            EventHandler<DescentCompletedEventArgs> temp = DescentCompleted;
            if (temp != null)
                temp(this, args);
        }

		public void DoDescentOperationOnNotExistingComputers( IEnumerable<TargetUidComputerPair> computers, string login, string password, bool withDelayedComputers, InstallPackageInfo package, out Guid operationUid, string scanScope, string domain )
		{
            // This is done in order not to include offline computers into the DescentResult below. Optimization is needed!
            var onlineTargetUidComputerPairs = (from pair in computers where pair.Computer.IsOnline && !(pair.State is Exception) select pair).ToArray();

            DescentResult result = new DescentResult(onlineTargetUidComputerPairs, withDelayedComputers, package);
			operationUid = result.OperationUid;
			lock( _descentResults )
			{
				_descentResults[result.OperationUid] = result;
			}

            _scanResultsPerOperation.Clear();
		    var operationContext = new AptScanOperationContext(){ OperationUid = result.OperationUid, Started = DateTime.UtcNow, TargetsSourceName = domain };
            _scanResultsPerOperation.TryAdd(result.OperationUid, operationContext);
            _descentMod = new DescentMod(DescentWatcherFactory, _catalog, operationContext.ScanResults);

			LogHelper.Trace( TraceEventType.Verbose, TraceCategory.Descent,
				"Deployment operation started{0} UID: {1}{0} computers count: {2}{0} with delayed targets: {3}{0}",
                Environment.NewLine, operationUid, onlineTargetUidComputerPairs.Count(), withDelayedComputers);

			var watchers = new ConcurrentDictionary<Guid, IDescentWatcher<ComputerDescentWatcherParams>>();
			var watchersContainer = new ComputerWatchersContainer( watchers );

            #region CIS installation related stuff
            //if( package != null && package.PackageUids != null && package.PackageUids.Length > 0 )
            //{
            //    object sync = new object();
            //    HashSet<long> tasks = new HashSet<long>();
            //    HashSet<Guid> taskRunnedComputers = new HashSet<Guid>();
            //    ConcurrentDictionary<Guid, string[]> computerSupportedIncompatibleProds = new ConcurrentDictionary<Guid, string[]>();
            //    ConcurrentDictionary<Guid, string[]> computerUnsupportedIncompatibleProds = new ConcurrentDictionary<Guid, string[]>();

            //    #region Action Became Deferred Delegate
            //    ActionDeferredListener actionDeferredListener = new ActionDeferredListener( result, package, sync, tasks );
            //    EventHandler<ActionDeferredEventArgs> actionDeferredDelegate = actionDeferredListener.ActionDeferred;
            //    #endregion

            //    #region Action Completed Delegate
            //    PackageInstallCompletedListener packageInstallCompletedListener = new PackageInstallCompletedListener( result, tasks, sync );
            //    IncompatibleUninstallCompletedListener incompatibleUninstallCompletedListener = new IncompatibleUninstallCompletedListener( result, tasks, sync,
            //        computerSupportedIncompatibleProds, computerUnsupportedIncompatibleProds );

            //    CisUninstallCompletedListener cisUninstallCompletedListener = new CisUninstallCompletedListener( result, tasks, sync );
            //    EventHandler<ActionCompletedEventArgs> packageInstallCompletedDelegate = packageInstallCompletedListener.ActionCompleted;
            //    EventHandler<ActionCompletedEventArgs> incompatibleUninstallCompletedDelegate = incompatibleUninstallCompletedListener.ActionCompleted;
            //    EventHandler<ActionCompletedEventArgs> cisUninstallCompletedDelegate = cisUninstallCompletedListener.ActionCompleted;
            //    #endregion

            //    #region Computer Connected Delegate

            //    ComputerConnectedListener computerConnectedListener = new ComputerConnectedListener( _directory, _tasks, _discovery, _productManagement,
            //        _tracker, _dal, result, sync, watchersContainer, taskRunnedComputers, tasks, package.Clone(),
            //        computerSupportedIncompatibleProds, computerUnsupportedIncompatibleProds, cisUninstallCompletedListener );

            //    EventHandler<ProductRegisteredEventArgs> computerConnectedDelegate = computerConnectedListener.ProductRegistered;
            //    #endregion

            //    #region Agent Descent Installation Completed Delegate
            //    AgentDescentCompletedListener agentAgentDescentCompletedListener = new AgentDescentCompletedListener( result, watchersContainer, DescentWatcherFactory, _tracker, computerConnectedListener );
            //    Action<IComputer, bool> agentDescentCompletedDelegate = agentAgentDescentCompletedListener.AgentDescentCompleted;
            //    #endregion

            //    #region Action Started/Completed Delegate
            //    AgentControlActionStartedListener agentControlActionStartedListener = new AgentControlActionStartedListener( result, computerConnectedListener, agentAgentDescentCompletedListener );
            //    //AgentControlActionCompletedListener agentControlActionCompletedListener = new AgentControlActionCompletedListener( result, computerConnectedListener );
            //    EventHandler<ActionStartedEventArgs> agentControlActionStartedDelegate = agentControlActionStartedListener.ActionChanged;
            //    //EventHandler<ActionCompletedEventArgs> agentControlActionCompletedDelegate = agentControlActionCompletedListener.ActionCompleted;
            //    #endregion

            //    #region Descent Package Installation Completed Delegate
            //    DescentCompletedListener descentCompletedListener = new DescentCompletedListener( watchersContainer, _tasks, _messaging, result,
            //        agentDescentCompletedDelegate, actionDeferredDelegate, computerConnectedDelegate,
            //        new[] { packageInstallCompletedDelegate, incompatibleUninstallCompletedDelegate, cisUninstallCompletedDelegate/*, agentControlActionCompletedDelegate*/ },
            //        agentControlActionStartedDelegate );

            //    System.Action installPackageDescentCompletedDelegate = descentCompletedListener.DecsentCompleted;
            //    descentCompletedListener.SetDescentCompletedDelegate( installPackageDescentCompletedDelegate );
            //    #endregion

            //    _tasks.ActionCompleted += packageInstallCompletedDelegate;
            //    _tasks.ActionCompleted += incompatibleUninstallCompletedDelegate;
            //    _tasks.ActionCompleted += cisUninstallCompletedDelegate;
            //    //_tasks.ActionCompleted += agentControlActionCompletedDelegate;
            //    _tasks.ActionChanged += agentControlActionStartedDelegate;

            //    // Commented out: does not work good for CIS update (shows 100% immediately before 1st reboot)
            //    //				_tasks.ActionDeferred += actionDeferredDelegate;
            //    _messaging.ProductRegistered += computerConnectedDelegate;
            //    result.AgentDescentCompleted += agentDescentCompletedDelegate;
            //    result.DescentCompleted += installPackageDescentCompletedDelegate;
            //}
            #endregion

            #region Descent Operation Completed Delegate
            System.Action wholeDescentCompletedDelegate = null;
			wholeDescentCompletedDelegate = () =>
			{
                LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent, "DescentCompleted event raised. Operation UID: {0}", result.OperationUid);

                // ===================================================================================================================
                // Deployment finalization
                int failed, notPerformed, succeeded, total, canceled;
                CalculateOperationStatistics(operationContext.ScanResults.Values, out failed, out notPerformed, out succeeded, out total, out canceled);

                if (operationContext.OperationUid == result.OperationUid)
                {
                    operationContext.Finished = DateTime.UtcNow;
                    operationContext.TotalTargetsCount = total;
                    operationContext.SucceededTargetsCount = succeeded;
                    operationContext.FailedTargetsCount = failed;
                    operationContext.NotPerformedTargetsCount = notPerformed;
                    operationContext.CanceledTargetsCount = canceled;
                }

                OnDescentCompleted(new DescentCompletedEventArgs() { OperationUid = result.OperationUid, Context = operationContext });
                // ===================================================================================================================

                _tracker.UnregisterListener(result);
                _tracker.UnregisterListener(operationContext);
                result.DescentCompleted -= wholeDescentCompletedDelegate;
			};
			#endregion

			_tracker.RegisterListener( result );
            _tracker.RegisterListener( operationContext );
			result.DescentCompleted += wholeDescentCompletedDelegate;

			var descentTargets = computers.Where( item => !( item.State is Exception ) ).Select( item => item.Computer ).ToArray();

            //===============================================================================================================================================
            // Add not available (offline) computers into the ScanResults dictionary with "NotPerformed" status - means the scan won't be performed on them.
            // They are required for statistics only
		    operationContext.TotalTargetsCount = descentTargets.Length;
		    List<IComputer> onlineComputers = new List<IComputer>();
		    foreach (var computer in descentTargets)
		    {
                ScanResultType scanResultType = ScanResultType.NotPerformed;

                if (computer.IsOnline)
                {
                    scanResultType = ScanResultType.InProgress;
                    onlineComputers.Add(computer);
                }
                operationContext.ScanResults.TryAdd(computer.Uid, new ScanResult() { HostName = computer.Name, IpAddress = computer.IPAddress.ToString(), ResultType = scanResultType, TargetUid = computer.Uid, OrganizationUnit = computer.OrganizationUnit });
		    }

            if (onlineComputers.Count == 0)
            {
                _tracker.UnregisterListener(result);
                _tracker.UnregisterListener(operationContext);
                result.DescentCompleted -= wholeDescentCompletedDelegate;
                throw new NoAvailableTargetsException();
            }
		    //===============================================================================================================================================

            DoDescendOperation(result, true, onlineComputers.ToArray(), login, password, false, scanScope);
		}

	    private static void CalculateOperationStatistics(IEnumerable<ScanResult> results, out int failed, out int notPerformed, out int succeeded, out int total, out int canceled)
        {
            notPerformed = 0;
            succeeded = 0;
            failed = 0;
            canceled = 0;

            total = results.Count();
            var groupped = from result in results group result by result.ResultType into temp select temp;
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
		
		public string GetDescentErrorMessage( string errorCode, int systemCode, string systemMessage, string[] extensionTexts, bool isUninstall,CultureInfo cultureInfo )
		{
			DescentStatus status;
			if( !Enum.TryParse( errorCode, out status ) )
			{
				throw new FormatException( String.Format( "Specified value '{0}' is not correct DescentStatus.", errorCode ) );
			}

			if( status == DescentStatus.status_success || status == DescentStatus.status_success_agent_updated )
			{
				return String.Empty;
			}

			ResourceText baseText = isUninstall ?
				ServerSideMessagesResources.DescentUninstallAgentFailedTinyFormat :
				ServerSideMessagesResources.DescentInstallAgentFailedTinyFormat;

			FormattedText shortMessage;
            var messageTranslatorCollection = MessageTranslatorCollection.Instance;
            FormattedText descriptionText = RemoteInstallerJob.GetDescentStatusDescription(status, extensionTexts, cultureInfo, messageTranslatorCollection, out shortMessage);
		    
            FormattedText text = new FormattedText(baseText, descriptionText, systemMessage, errorCode, systemCode);

            return text.ToString(messageTranslatorCollection, cultureInfo);
		}

		public DescentResult GetDescentResult( Guid operationUid )
		{
			DescentResult result;
			lock( _descentResults )
			{
				_descentResults.TryGetValue( operationUid, out result );
			}

			return result;
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

	    public void Init( ISubSystemCatalog catalog )
		{
            // TODO: Commented
            //_resolver = catalog.Find<IDirectoryResolver>();
            //_messages = ( IMessagesListener )catalog.Find<IMessagingSubSystem>();
            //_messaging = catalog.Find<IMessagingSubSystem>();
            //_directory = catalog.Find<IDirectorySubSystem>();
            //_discovery = catalog.Find<IDiscoverySubSystem>();
            //_tasks = catalog.Find<ITasksSubSystem>();
            _tracker = catalog.Find<IChangeTrackingSubSystem>();
            //_productManagement = catalog.Find<IProductManagementSubSystem>();
            //_dal = catalog.Find<IEntityStorage>();

			_catalog = catalog;

			ushort socketPortNumber = GetSocketPortForAgent();
			byte[] publicKey = GetServerDsaPublicKey();
			_windowsPackageHelper = new WindowsAgentPackageHelper(Settings.Default.WebRootFolder, socketPortNumber, publicKey);
			_debianPackageHelper = new DebianAgentPackageHelper(Settings.Default.WebRootFolder, socketPortNumber, publicKey);
			_macosPackageHelper = new MacOSAgentPackageHelper(Settings.Default.WebRootFolder, socketPortNumber, publicKey);

			DescentWatcherFactory = new DescentWatcherFactory();
			//_descentMod = new DescentMod( DescentWatcherFactory, _catalog, _scanResults );
			_descentResultsCleanupTimer = new Timer( DescentResultsCleanupTimerCallback, null, Settings.Default.DescentResultLifetime, TimeSpan.FromMilliseconds( Timeout.Infinite ) );
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

		private void DescentResultsCleanupTimerCallback( object state )
		{
			try
			{
				LogHelper.Trace( TraceEventType.Verbose, TraceCategory.Descent, "Descent results cleanup running" );
				Debug.WriteLine( "" );

				_descentResultsCleanupTimer.Change( Timeout.Infinite, Timeout.Infinite );
				DateTime utcNow = DateTime.UtcNow;
				List<DescentResult> toBeRemoved = new List<DescentResult>();
				lock( _descentResults )
				{
					foreach( var descentResult in _descentResults.Values )
					{
						if( descentResult.Completed &&
							descentResult.CompletedTime != null &&
							utcNow - descentResult.CompletedTime.Value > Settings.Default.DescentResultLifetime )
						{
							toBeRemoved.Add( descentResult );
						}
					}

					foreach( var descentResult in toBeRemoved )
					{
						_descentResults.Remove( descentResult.OperationUid );

						LogHelper.Trace( TraceEventType.Verbose, TraceCategory.Descent,
							"Completed descent result removed{0} UID: {1}{0} completed (UTC): {2}{0}",
							Environment.NewLine, descentResult.OperationUid, descentResult.CompletedTime );
					}
				}

				_descentResultsCleanupTimer.Change( Settings.Default.DescentResultCleanupPeriod, TimeSpan.FromMilliseconds( Timeout.Infinite ) );
			}
			catch( ObjectDisposedException )
			{
				// No reaction in case when timer is disposed: it means the subsystem is disposed.
			}
		}

		#endregion
	}

    public class DescentCompletedEventArgs : EventArgs
    {
        public Guid OperationUid { get; set; }
        public AptScanOperationContext Context { get; set; }
    }
}