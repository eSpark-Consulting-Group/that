using System.Globalization;
using APTAT.DeploymentCore.Messages;

namespace APTAT.DeploymentCore.Properties
{
    public class ServerSideMessagesResources
    {
        public const string SourceName = @"CESM Server";

        private static readonly ExtendedResourceManager resourceMan =
            new ExtendedResourceManager("APTAT.DeploymentCore.Properties.ServerSideMessagesResources",
                typeof(ServerSideMessagesResources).Assembly);


        public static readonly ResourceText AcceptLiteral =
                resourceMan.GetResourceText(SourceName, "AcceptLiteral");

        public static readonly ResourceText InstallationTimeoutMessage =
                resourceMan.GetResourceText(SourceName, "InstallationTimeoutMessage");

        public static readonly ResourceText DeclineLiteral =
                resourceMan.GetResourceText(SourceName, "DeclineLiteral");

        public static readonly ResourceText NewVersionRequestMessageFormat =
                resourceMan.GetResourceText(SourceName, "NewVersionRequestMessageFormat");

        public static readonly ResourceText NoLiteral =
                    resourceMan.GetResourceText(SourceName, "NoLiteral");

        public static readonly ResourceText YesLiteral =
                    resourceMan.GetResourceText(SourceName, "YesLiteral");

        public static readonly ResourceText DirectoryComputerNotFoundFormat =
                    resourceMan.GetResourceText(SourceName, "DirectoryComputerNotFoundFormat");

        public static readonly ResourceText ComputerTryConnect =
                    resourceMan.GetResourceText(SourceName, "ComputerTryConnect");

        public static readonly ResourceText LicenseInvalidMessage =
                    resourceMan.GetResourceText(SourceName, "LicenseInvalidMessage");

        public static readonly ResourceText ProductsOverflowMessageFormat =
                    resourceMan.GetResourceText(SourceName, "ProductsOverflowMessageFormat");

        public static readonly ResourceText ExpirationInDaysWarningMessageFormat =
                    resourceMan.GetResourceText(SourceName, "ExpirationInDaysWarningMessageFormat");

        public static readonly ResourceText ExpirationInHoursWarningMessageFormat =
                    resourceMan.GetResourceText(SourceName, "ExpirationInHoursWarningMessageFormat");

        public static readonly ResourceText ComputersOverflowMessageFormat =
                    resourceMan.GetResourceText(SourceName, "ComputersOverflowMessageFormat");

        public static readonly ResourceText WarrantyOverflowMessageFormat =
                    resourceMan.GetResourceText(SourceName, "WarrantyOverflowMessageFormat");


        public static readonly ResourceText DescentInstallAgentSuccess =
                    resourceMan.GetResourceText(SourceName, "DescentInstallAgentSuccess");

        public static readonly ResourceText DescentInstallAgentSuccessUpdated =
                    resourceMan.GetResourceText(SourceName, "DescentInstallAgentSuccessUpdated");

        public static readonly ResourceText DescentUninstallAgentSuccess =
                    resourceMan.GetResourceText(SourceName, "DescentUninstallAgentSuccess");


        public static readonly ResourceText ComputerShoudBeManagedToPerformAgentInstallationMessage =
                    resourceMan.GetResourceText(SourceName, "ComputerShoudBeManagedToPerformAgentInstallationMessage");

        public static readonly ResourceText ComputerShoudBeManagedToPerformAgentUninstallationMessage =
                    resourceMan.GetResourceText(SourceName, "ComputerShoudBeManagedToPerformAgentUninstallationMessage");

        public static readonly ResourceText DirectoryComputerDnsDifferFormat =
                    resourceMan.GetResourceText(SourceName, "DirectoryComputerDnsDifferFormat");

        public static readonly ResourceText DirectoryComputerIPDifferFormat =
                    resourceMan.GetResourceText(SourceName, "DirectoryComputerIPDifferFormat");

        public static readonly ResourceText DirectoryComputerAgentVersionOutdatedFormat =
                    resourceMan.GetResourceText(SourceName, "DirectoryComputerAgentVersionOutdatedFormat");

        public static readonly ResourceText UpdateAvailableMessageFormat =
                    resourceMan.GetResourceText(SourceName, "UpdateAvailableMessageFormat");

        public static readonly ResourceText NetworkLostMessageFormat =
                    resourceMan.GetResourceText(SourceName, "NetworkLostMessageFormat");

        public static readonly ResourceText PackageDownloadedMessageFormat =
                    resourceMan.GetResourceText(SourceName, "PackageDownloadedMessageFormat");

        public static readonly ResourceText PackageDownloadingFailWithReasonMessageFormat =
                    resourceMan.GetResourceText(SourceName, "PackageDownloadingFailWithReasonMessageFormat");

        public static readonly ResourceText PackageDownloadingMessageFormat =
                    resourceMan.GetResourceText(SourceName, "PackageDownloadingMessageFormat");

        public static readonly ResourceText PackageNotFoundMessageFormat =
                    resourceMan.GetResourceText(SourceName, "PackageNotFoundMessageFormat");

        public static readonly ResourceText PackageFileIsCorruptedOrIsNotTheOriginalOneFormat =
                    resourceMan.GetResourceText(SourceName, "PackageFileIsCorruptedOrIsNotTheOriginalOneFormat");

        public static readonly ResourceText ProductIsAlreadyDownloadedMessageFormat =
                    resourceMan.GetResourceText(SourceName, "ProductIsAlreadyDownloadedMessageFormat");

        public static readonly ResourceText WarrantyDisabledOnComputerMessageFormat =
                    resourceMan.GetResourceText(SourceName, "WarrantyDisabledOnComputerMessageFormat");

        public static readonly ResourceText WarrantyExpiredWithSolutionMessageFormat =
                    resourceMan.GetResourceText(SourceName, "WarrantyExpiredWithSolutionMessageFormat");

        public static readonly ResourceText WarrantyActivatedOnComputerMessageFormat =
                    resourceMan.GetResourceText(SourceName, "WarrantyActivatedOnComputerMessageFormat");

        public static readonly ResourceText CannotEnableWarrantyOnComputerMessageFormat =
                    resourceMan.GetResourceText(SourceName, "CannotEnableWarrantyOnComputerMessageFormat");

        public static readonly ResourceText WarrantyRestoredOnComputerMessageFormat =
                    resourceMan.GetResourceText(SourceName, "WarrantyRestoredOnComputerMessageFormat");

        public static readonly ResourceText WarrantyCorruptedOnComputerMessageFormat =
                    resourceMan.GetResourceText(SourceName, "WarrantyCorruptedOnComputerMessageFormat");

        public static readonly ResourceText RequestAnswerFailedOnComputerMessageFormat =
                    resourceMan.GetResourceText(SourceName, "RequestAnswerFailedOnComputerMessageFormat");

        public static readonly ResourceText AgnCoreActionFactoryNotFound =
                resourceMan.GetResourceText(SourceName, "AgnCore_ActionFactoryNotFound");

        public static readonly ResourceText CesmIncompatibleAgentVersion =
                resourceMan.GetResourceText(SourceName, "CesmIncompatibleAgentVersion");

        public static readonly ResourceText CesmActionFailedNotRunningOnAgent =
                resourceMan.GetResourceText(SourceName, "CesmActionFailedNotRunningOnAgent");

        public static readonly ResourceText CesmOK =
                resourceMan.GetResourceText(SourceName, "CesmOK");

        public static readonly ResourceText CesmTimeout =
                resourceMan.GetResourceText(SourceName, "CesmTimeout");

        public static readonly ResourceText CesmComputerIsAlreadyOnlineFormat =
                resourceMan.GetResourceText(SourceName, "CesmComputerIsAlreadyOnlineFormat");

        public static readonly ResourceText CesmUnknownMacAddress =
                resourceMan.GetResourceText(SourceName, "CesmUnknownMacAddress");

        public static readonly ResourceText CesmFailedToSendNetworkPacket =
                resourceMan.GetResourceText(SourceName, "CesmFailedToSendNetworkPacket");

        public static readonly ResourceText CesmActualStateIsUnknownServerError =
                resourceMan.GetResourceText(SourceName, "CesmActualStateIsUnknownServerError");

        public static readonly ResourceText AgentUninstallationFailedMessage =
                resourceMan.GetResourceText(SourceName, "AgentUninstallationFailedMessage");

        public static readonly ResourceText AgentUninstallationSucceededMessage =
                resourceMan.GetResourceText(SourceName, "AgentUninstallationSucceededMessage");

        public static readonly ResourceText AgentIsUptodateMessage =
                resourceMan.GetResourceText(SourceName, "AgentIsUptodateMessage");

        public static readonly ResourceText AgentUpdateFailedMessage =
                resourceMan.GetResourceText(SourceName, "AgentUpdateFailedMessage");


        public static readonly ResourceText IgnoreExternalWarrantyStatus =
                resourceMan.GetResourceText(SourceName, "IgnoreExternalWarrantyStatus");

        public static readonly ResourceText OkExternalWarrantyStatus =
                resourceMan.GetResourceText(SourceName, "OkExternalWarrantyStatus");

        public static readonly ResourceText PendingExternalWarrantyStatus =
                resourceMan.GetResourceText(SourceName, "PendingExternalWarrantyStatus");

        public static readonly ResourceText WarrantyAvBasesOutdatedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyAvBasesOutdatedMessage");

        public static readonly ResourceText WarrantyCisFailedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyCisFailedMessage");

        public static readonly ResourceText WarrantyComputerInfectedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyComputerInfectedMessage");

        public static readonly ResourceText WarrantyLicenseExpiredMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyLicenseExpiredMessage");

        public static readonly ResourceText WarrantyProblemAvBasesOutdatedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemAvBasesOutdatedMessage");

        public static readonly ResourceText WarrantyProblemAvNotInstalledMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemAvNotInstalledMessage");

        public static readonly ResourceText WarrantyProblemAvNotAllCisEnginesReadyMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemAvNotAllCisEnginesReadyMessage");

        public static readonly ResourceText WarrantyProblemAntivirusDisabledMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemAntivirusDisabledMessage");

        public static readonly ResourceText WarrantyProblemDefensePlusDisabledMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemDefensePlusDisabledMessage");

        public static readonly ResourceText WarrantyProblemFirewallDisabledMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemFirewallDisabledMessage");

        public static readonly ResourceText WarrantyProblemCisInLocalModeMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemCisInLocalModeMessage");

        public static readonly ResourceText WarrantyProblemCispublicErrorMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemCispublicErrorMessage");

        public static readonly ResourceText WarrantyProblemCisNotConnectedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemCisNotConnectedMessage");

        public static readonly ResourceText WarrantyProblemComputerInfectedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemComputerInfectedMessage");

        public static readonly ResourceText WarrantyProblemLicenseExpiredMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemLicenseExpiredMessage");

        public static readonly ResourceText WarrantyProblemLoggedOffModeMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemLoggedOffModeMessage");

        public static readonly ResourceText WarrantyProblemNotSupportedInCisMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemNotSupportedInCisMessage");

        public static readonly ResourceText WarrantyProblemScanFailedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemScanFailedMessage");

        public static readonly ResourceText WarrantyProblemUnknownErrorMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemUnknownErrorMessage");

        public static readonly ResourceText WarrantyUndefinedErrorMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyUndefinedErrorMessage");

        public static readonly ResourceText UnknownErrorMessage =
               resourceMan.GetResourceText(SourceName, "UnknownErrorMessage");

        public static readonly ResourceText UnknownProductMessage =
                resourceMan.GetResourceText(SourceName, "UnknownProductMessage");

        public static readonly ResourceText CannotResolveHostMessage =
               resourceMan.GetResourceText(SourceName, "CannotResolveHostMessage");
        public static readonly ResourceText CannotResolveHostSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "CannotResolveHostSuggestionMessage");
        public static readonly ResourceText LoginProblemMessage =
               resourceMan.GetResourceText(SourceName, "LoginProblemMessage");
        public static readonly ResourceText LoginProblemSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "LoginProblemSuggestionMessage");
        public static readonly ResourceText InvalidUserNameMessage =
               resourceMan.GetResourceText(SourceName, "InvalidUserNameMessage");
        public static readonly ResourceText InvalidUserNameOrPasswordMessage =
               resourceMan.GetResourceText(SourceName, "InvalidUserNameOrPasswordMessage");
        public static readonly ResourceText InvalidUserNameOrPasswordSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "InvalidUserNameOrPasswordSuggestionMessage");
        public static readonly ResourceText TargetComputerNotRespondingMessage =
               resourceMan.GetResourceText(SourceName, "TargetComputerNotRespondingMessage");
        public static readonly ResourceText TargetComputerNotRespondingSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "TargetComputerNotRespondingSuggestionMessage");
        public static readonly ResourceText TrustedRelationshipFailureMessage =
               resourceMan.GetResourceText(SourceName, "TrustedRelationshipFailureMessage");
        public static readonly ResourceText AgentIsAlreadyInstalledAndServerDiffersMessage =
               resourceMan.GetResourceText(SourceName, "AgentIsAlreadyInstalledAndServerDiffersMessage");
        public static readonly ResourceText AgentIsAlreadyInstalledAndServerDiffersSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "AgentIsAlreadyInstalledAndServerDiffersSuggestionMessage");
        public static readonly ResourceText AgentIsAlreadyInstalledMessage =
               resourceMan.GetResourceText(SourceName, "AgentIsAlreadyInstalledMessage");
        public static readonly ResourceText AgentIsNotInstalledMessage =
               resourceMan.GetResourceText(SourceName, "AgentIsNotInstalledMessage");
        public static readonly ResourceText FatalInstallationErrorMessage =
               resourceMan.GetResourceText(SourceName, "FatalInstallationErrorMessage");
        public static readonly ResourceText HostNotAccessibleMessage =
               resourceMan.GetResourceText(SourceName, "HostNotAccessibleMessage");
        public static readonly ResourceText HostNotAccessibleSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "HostNotAccessibleSuggestionMessage");
        public static readonly ResourceText CannotStartRemoteServiceMessage =
               resourceMan.GetResourceText(SourceName, "CannotStartRemoteServiceMessage");
        public static readonly ResourceText CannotSetupDescentServiceSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "CannotSetupDescentServiceSuggestionMessage");
        public static readonly ResourceText CannotSetupDescentServiceMessage =
               resourceMan.GetResourceText(SourceName, "CannotSetupDescentServiceMessage");
        public static readonly ResourceText CannotRemoveRemoteServiceMessage =
               resourceMan.GetResourceText(SourceName, "CannotRemoveRemoteServiceMessage");
        public static readonly ResourceText CannotCopyFileMessage =
               resourceMan.GetResourceText(SourceName, "CannotCopyFileMessage");
        public static readonly ResourceText CannotCopyFileSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "CannotCopyFileSuggestionMessage");
        public static readonly ResourceText AnotherInstallationInProgressSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "AnotherInstallationInProgressSuggestionMessage");
        public static readonly ResourceText AnotherInstallationInProgressMessage =
               resourceMan.GetResourceText(SourceName, "AnotherInstallationInProgressMessage");
        public static readonly ResourceText AgentDescentTimeoutMessage =
               resourceMan.GetResourceText(SourceName, "AgentDescentTimeoutMessage");
        public static readonly ResourceText AgentDescentTimeoutSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "AgentDescentTimeoutSuggestionMessage");
        public static readonly ResourceText NoCompatibleMsiFoundMessage =
               resourceMan.GetResourceText(SourceName, "NoCompatibleMsiFoundMessage");
        public static readonly ResourceText ProductAlreadyInstalledMessage =
               resourceMan.GetResourceText(SourceName, "ProductAlreadyInstalledMessage");
        public static readonly ResourceText UnsupportedErrorMessage =
               resourceMan.GetResourceText(SourceName, "UnsupportedErrorMessage");
        public static readonly ResourceText InvalidResponseMessage =
               resourceMan.GetResourceText(SourceName, "InvalidResponseMessage");
        public static readonly ResourceText DescentStatusDescriptionWithSuggestionFormat =
               resourceMan.GetResourceText(SourceName, "DescentStatusDescriptionWithSuggestionFormat");
        public static readonly ResourceText ErrorCodeMessageFormat =
               resourceMan.GetResourceText(SourceName, "ErrorCodeMessageFormat");
        public static readonly ResourceText DescentInstallAgentFailedFormat =
               resourceMan.GetResourceText(SourceName, "DescentInstallAgentFailedFormat");
        public static readonly ResourceText DescentUninstallAgentFailedFormat =
               resourceMan.GetResourceText(SourceName, "DescentUninstallAgentFailedFormat");
        public static readonly ResourceText DescentInstallAgentFailedTinyFormat =
               resourceMan.GetResourceText(SourceName, "DescentInstallAgentFailedTinyFormat");
        public static readonly ResourceText DescentUninstallAgentFailedTinyFormat =
               resourceMan.GetResourceText(SourceName, "DescentUninstallAgentFailedTinyFormat");
        public static readonly ResourceText DescentQueryAgentInfoFailedFormat =
               resourceMan.GetResourceText(SourceName, "DescentQueryAgentInfoFailedFormat");
        public static readonly ResourceText DescentTimeoutSystemMessage =
               resourceMan.GetResourceText(SourceName, "DescentTimeoutSystemMessage");
        public static readonly ResourceText PackageDescentTimeoutMessage =
               resourceMan.GetResourceText(SourceName, "PackageDescentTimeoutMessage");
        public static readonly ResourceText TargetComputerNotManagedMessage =
               resourceMan.GetResourceText(SourceName, "TargetComputerNotManagedMessage");
        public static readonly ResourceText DescentPackageCompletedRebootRequired =
                resourceMan.GetResourceText(SourceName, "DescentPackageCompletedRebootRequired");
        public static readonly ResourceText DescentPackageCompletedRebootingInProgress =
                resourceMan.GetResourceText(SourceName, "DescentPackageCompletedRebootingInProgress");
        public static readonly ResourceText DescentPackageCompleted =
                resourceMan.GetResourceText(SourceName, "DescentPackageCompleted");
        public static readonly ResourceText DescentProgressAgentInstallConnectingToTarget =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallConnectingToTarget");
        public static readonly ResourceText DescentProgressAgentInstallCopyingHelperServicePackage =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallCopyingHelperServicePackage");
        public static readonly ResourceText DescentProgressAgentInstallConfiguringHelperService =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallConfiguringHelperService");
        public static readonly ResourceText DescentProgressAgentInstallStartingHelperService =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallStartingHelperService");
        public static readonly ResourceText DescentProgressAgentInstallSearchingExistingInstallations =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallSearchingExistingInstallations");
        public static readonly ResourceText DescentProgressAgentInstallCopyingAgentPackage =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallCopyingAgentPackage");
        public static readonly ResourceText DescentProgressAgentInstallInstallingAgentPackage =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallInstallingAgentPackage");
        public static readonly ResourceText DescentProgressAgentInstallFinalizingAgentInstallation =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallFinalizingAgentInstallation");
        public static readonly ResourceText DescentUninstallIncompatibleUnsupportedMessage =
                resourceMan.GetResourceText(SourceName, "DescentUninstallIncompatibleUnsupportedMessage");
        public static readonly ResourceText DescentUninstallIncompatibleUnsupportedSuggestionMessage =
                resourceMan.GetResourceText(SourceName, "DescentUninstallIncompatibleUnsupportedSuggestionMessage");
        public static readonly ResourceText DescentUninstallIncompatiblePartiallyUnsupportedMessage =
                resourceMan.GetResourceText(SourceName, "DescentUninstallIncompatiblePartiallyUnsupportedMessage");
        public static readonly ResourceText DescentUninstallIncompatiblePartiallyUnsupportedSuggestionMessage =
                resourceMan.GetResourceText(SourceName, "DescentUninstallIncompatiblePartiallyUnsupportedSuggestionMessage");
        public static readonly ResourceText DescentOutdatedCisUninstallDiscoveryMessage =
                resourceMan.GetResourceText(SourceName, "DescentOutdatedCisUninstallDiscoveryMessage");
        public static readonly ResourceText DescentOutdatedCisUninstallRemovingMessage =
                resourceMan.GetResourceText(SourceName, "DescentOutdatedCisUninstallRemovingMessage");
        public static readonly ResourceText DescentAgentWaitingConnectionMessage =
                resourceMan.GetResourceText(SourceName, "DescentAgentWaitingConnectionMessage");
        public static readonly ResourceText DescentUninstallIncompatibleDiscoveryMessage =
                resourceMan.GetResourceText(SourceName, "DescentUninstallIncompatibleDiscoveryMessage");

        public static readonly ResourceText DescentUninstallOutdatedPackageMessage =
                resourceMan.GetResourceText(SourceName, "DescentUninstallOutdatedPackageMessage");
        public static readonly ResourceText DescentInstallPackageMessage =
                resourceMan.GetResourceText(SourceName, "DescentInstallPackageMessage");

        public static readonly ResourceText InternetAccessDescriptionFormatMessage =
                resourceMan.GetResourceText(SourceName, "InternetAccessDescriptionFormatMessage");

        public static readonly ResourceText CisRequiresRebootingAfterScanMessage =
                resourceMan.GetResourceText(SourceName, "CisRequiresRebootingAfterScanMessage");

        public static readonly ResourceText ScanTerminatedUnexpectedlyMessage =
                resourceMan.GetResourceText(SourceName, "ScanTerminatedUnexpectedlyMessage");

        public static readonly ResourceText DescentFormatDescriptionWithoutSuggestion =
                resourceMan.GetResourceText(SourceName, "DescentFormatDescriptionWithoutSuggestion");

        public static string GetString(string name)
        {
            return resourceMan.GetString(name);
        }
        public static string GetString(string name, CultureInfo cultureInfo)
        {
            return resourceMan.GetString(name, cultureInfo);
        }
    }
}
