using System.Globalization;
using APTAT.Core;

namespace APTAT.Core.Properties
{
    internal class ServerSideMessagesResources
    {
        public const string SourceName = @"CESM Server";

        private static readonly ExtendedResourceManager resourceMan =
            new ExtendedResourceManager("APTAT.Core.Properties.ServerSideMessagesResources",
                typeof(ServerSideMessagesResources).Assembly);


        internal static readonly ResourceText AcceptLiteral =
                resourceMan.GetResourceText(SourceName, "AcceptLiteral");

        internal static readonly ResourceText DeclineLiteral =
                resourceMan.GetResourceText(SourceName, "DeclineLiteral");

        internal static readonly ResourceText NewVersionRequestMessageFormat =
                resourceMan.GetResourceText(SourceName, "NewVersionRequestMessageFormat");

        internal static readonly ResourceText NoLiteral =
                    resourceMan.GetResourceText(SourceName, "NoLiteral");

        internal static readonly ResourceText YesLiteral =
                    resourceMan.GetResourceText(SourceName, "YesLiteral");

        internal static readonly ResourceText DirectoryComputerNotFoundFormat =
                    resourceMan.GetResourceText(SourceName, "DirectoryComputerNotFoundFormat");

        internal static readonly ResourceText ComputerTryConnect =
                    resourceMan.GetResourceText(SourceName, "ComputerTryConnect");

        internal static readonly ResourceText LicenseInvalidMessage =
                    resourceMan.GetResourceText(SourceName, "LicenseInvalidMessage");

        internal static readonly ResourceText ProductsOverflowMessageFormat =
                    resourceMan.GetResourceText(SourceName, "ProductsOverflowMessageFormat");

        internal static readonly ResourceText ExpirationInDaysWarningMessageFormat =
                    resourceMan.GetResourceText(SourceName, "ExpirationInDaysWarningMessageFormat");

        internal static readonly ResourceText ExpirationInHoursWarningMessageFormat =
                    resourceMan.GetResourceText(SourceName, "ExpirationInHoursWarningMessageFormat");

        internal static readonly ResourceText ComputersOverflowMessageFormat =
                    resourceMan.GetResourceText(SourceName, "ComputersOverflowMessageFormat");

        internal static readonly ResourceText WarrantyOverflowMessageFormat =
                    resourceMan.GetResourceText(SourceName, "WarrantyOverflowMessageFormat");


        internal static readonly ResourceText DescentInstallAgentSuccess =
                    resourceMan.GetResourceText(SourceName, "DescentInstallAgentSuccess");

        internal static readonly ResourceText DescentInstallAgentSuccessUpdated =
                    resourceMan.GetResourceText(SourceName, "DescentInstallAgentSuccessUpdated");

        internal static readonly ResourceText DescentUninstallAgentSuccess =
                    resourceMan.GetResourceText(SourceName, "DescentUninstallAgentSuccess");


        internal static readonly ResourceText ComputerShoudBeManagedToPerformAgentInstallationMessage =
                    resourceMan.GetResourceText(SourceName, "ComputerShoudBeManagedToPerformAgentInstallationMessage");

        internal static readonly ResourceText ComputerShoudBeManagedToPerformAgentUninstallationMessage =
                    resourceMan.GetResourceText(SourceName, "ComputerShoudBeManagedToPerformAgentUninstallationMessage");

        internal static readonly ResourceText DirectoryComputerDnsDifferFormat =
                    resourceMan.GetResourceText(SourceName, "DirectoryComputerDnsDifferFormat");

        internal static readonly ResourceText DirectoryComputerIPDifferFormat =
                    resourceMan.GetResourceText(SourceName, "DirectoryComputerIPDifferFormat");

        internal static readonly ResourceText DirectoryComputerAgentVersionOutdatedFormat =
                    resourceMan.GetResourceText(SourceName, "DirectoryComputerAgentVersionOutdatedFormat");

        internal static readonly ResourceText UpdateAvailableMessageFormat =
                    resourceMan.GetResourceText(SourceName, "UpdateAvailableMessageFormat");

        internal static readonly ResourceText NetworkLostMessageFormat =
                    resourceMan.GetResourceText(SourceName, "NetworkLostMessageFormat");

        internal static readonly ResourceText PackageDownloadedMessageFormat =
                    resourceMan.GetResourceText(SourceName, "PackageDownloadedMessageFormat");

        internal static readonly ResourceText PackageDownloadingFailWithReasonMessageFormat =
                    resourceMan.GetResourceText(SourceName, "PackageDownloadingFailWithReasonMessageFormat");

        internal static readonly ResourceText PackageDownloadingMessageFormat =
                    resourceMan.GetResourceText(SourceName, "PackageDownloadingMessageFormat");

        internal static readonly ResourceText PackageNotFoundMessageFormat =
                    resourceMan.GetResourceText(SourceName, "PackageNotFoundMessageFormat");

        internal static readonly ResourceText PackageFileIsCorruptedOrIsNotTheOriginalOneFormat =
                    resourceMan.GetResourceText(SourceName, "PackageFileIsCorruptedOrIsNotTheOriginalOneFormat");

        internal static readonly ResourceText ProductIsAlreadyDownloadedMessageFormat =
                    resourceMan.GetResourceText(SourceName, "ProductIsAlreadyDownloadedMessageFormat");

        internal static readonly ResourceText WarrantyDisabledOnComputerMessageFormat =
                    resourceMan.GetResourceText(SourceName, "WarrantyDisabledOnComputerMessageFormat");

        internal static readonly ResourceText WarrantyExpiredWithSolutionMessageFormat =
                    resourceMan.GetResourceText(SourceName, "WarrantyExpiredWithSolutionMessageFormat");

        internal static readonly ResourceText WarrantyActivatedOnComputerMessageFormat =
                    resourceMan.GetResourceText(SourceName, "WarrantyActivatedOnComputerMessageFormat");

        internal static readonly ResourceText CannotEnableWarrantyOnComputerMessageFormat =
                    resourceMan.GetResourceText(SourceName, "CannotEnableWarrantyOnComputerMessageFormat");

        internal static readonly ResourceText WarrantyRestoredOnComputerMessageFormat =
                    resourceMan.GetResourceText(SourceName, "WarrantyRestoredOnComputerMessageFormat");

        internal static readonly ResourceText WarrantyCorruptedOnComputerMessageFormat =
                    resourceMan.GetResourceText(SourceName, "WarrantyCorruptedOnComputerMessageFormat");

        internal static readonly ResourceText RequestAnswerFailedOnComputerMessageFormat =
                    resourceMan.GetResourceText(SourceName, "RequestAnswerFailedOnComputerMessageFormat");

        internal static readonly ResourceText AgnCoreActionFactoryNotFound =
                resourceMan.GetResourceText(SourceName, "AgnCore_ActionFactoryNotFound");

        internal static readonly ResourceText CesmIncompatibleAgentVersion =
                resourceMan.GetResourceText(SourceName, "CesmIncompatibleAgentVersion");

        internal static readonly ResourceText CesmActionFailedNotRunningOnAgent =
                resourceMan.GetResourceText(SourceName, "CesmActionFailedNotRunningOnAgent");

        internal static readonly ResourceText CesmOK =
                resourceMan.GetResourceText(SourceName, "CesmOK");

        internal static readonly ResourceText CesmTimeout =
                resourceMan.GetResourceText(SourceName, "CesmTimeout");

        internal static readonly ResourceText CesmComputerIsAlreadyOnlineFormat =
                resourceMan.GetResourceText(SourceName, "CesmComputerIsAlreadyOnlineFormat");

        internal static readonly ResourceText CesmUnknownMacAddress =
                resourceMan.GetResourceText(SourceName, "CesmUnknownMacAddress");

        internal static readonly ResourceText CesmFailedToSendNetworkPacket =
                resourceMan.GetResourceText(SourceName, "CesmFailedToSendNetworkPacket");

        internal static readonly ResourceText CesmActualStateIsUnknownServerError =
                resourceMan.GetResourceText(SourceName, "CesmActualStateIsUnknownServerError");

        internal static readonly ResourceText AgentUninstallationFailedMessage =
                resourceMan.GetResourceText(SourceName, "AgentUninstallationFailedMessage");

        internal static readonly ResourceText AgentUninstallationSucceededMessage =
                resourceMan.GetResourceText(SourceName, "AgentUninstallationSucceededMessage");

        internal static readonly ResourceText AgentIsUptodateMessage =
                resourceMan.GetResourceText(SourceName, "AgentIsUptodateMessage");

        internal static readonly ResourceText AgentUpdateFailedMessage =
                resourceMan.GetResourceText(SourceName, "AgentUpdateFailedMessage");


        internal static readonly ResourceText IgnoreExternalWarrantyStatus =
                resourceMan.GetResourceText(SourceName, "IgnoreExternalWarrantyStatus");

        internal static readonly ResourceText OkExternalWarrantyStatus =
                resourceMan.GetResourceText(SourceName, "OkExternalWarrantyStatus");

        internal static readonly ResourceText PendingExternalWarrantyStatus =
                resourceMan.GetResourceText(SourceName, "PendingExternalWarrantyStatus");

        internal static readonly ResourceText WarrantyAvBasesOutdatedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyAvBasesOutdatedMessage");

        internal static readonly ResourceText WarrantyCisFailedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyCisFailedMessage");

        internal static readonly ResourceText WarrantyComputerInfectedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyComputerInfectedMessage");

        internal static readonly ResourceText WarrantyLicenseExpiredMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyLicenseExpiredMessage");

        internal static readonly ResourceText WarrantyProblemAvBasesOutdatedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemAvBasesOutdatedMessage");

        internal static readonly ResourceText WarrantyProblemAvNotInstalledMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemAvNotInstalledMessage");

        internal static readonly ResourceText WarrantyProblemAvNotAllCisEnginesReadyMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemAvNotAllCisEnginesReadyMessage");

        internal static readonly ResourceText WarrantyProblemAntivirusDisabledMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemAntivirusDisabledMessage");

        internal static readonly ResourceText WarrantyProblemDefensePlusDisabledMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemDefensePlusDisabledMessage");

        internal static readonly ResourceText WarrantyProblemFirewallDisabledMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemFirewallDisabledMessage");

        internal static readonly ResourceText WarrantyProblemCisInLocalModeMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemCisInLocalModeMessage");

        internal static readonly ResourceText WarrantyProblemCisInternalErrorMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemCisInternalErrorMessage");

        internal static readonly ResourceText WarrantyProblemCisNotConnectedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemCisNotConnectedMessage");

        internal static readonly ResourceText WarrantyProblemComputerInfectedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemComputerInfectedMessage");

        internal static readonly ResourceText WarrantyProblemLicenseExpiredMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemLicenseExpiredMessage");

        internal static readonly ResourceText WarrantyProblemLoggedOffModeMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemLoggedOffModeMessage");

        internal static readonly ResourceText WarrantyProblemNotSupportedInCisMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemNotSupportedInCisMessage");

        internal static readonly ResourceText WarrantyProblemScanFailedMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemScanFailedMessage");

        internal static readonly ResourceText WarrantyProblemUnknownErrorMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyProblemUnknownErrorMessage");

        internal static readonly ResourceText WarrantyUndefinedErrorMessage =
                resourceMan.GetResourceText(SourceName, "WarrantyUndefinedErrorMessage");

        internal static readonly ResourceText UnknownErrorMessage =
               resourceMan.GetResourceText(SourceName, "UnknownErrorMessage");

        internal static readonly ResourceText UnknownProductMessage =
                resourceMan.GetResourceText(SourceName, "UnknownProductMessage");

        internal static readonly ResourceText CannotResolveHostMessage =
               resourceMan.GetResourceText(SourceName, "CannotResolveHostMessage");
        internal static readonly ResourceText CannotResolveHostSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "CannotResolveHostSuggestionMessage");
        internal static readonly ResourceText LoginProblemMessage =
               resourceMan.GetResourceText(SourceName, "LoginProblemMessage");
        internal static readonly ResourceText LoginProblemSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "LoginProblemSuggestionMessage");
        internal static readonly ResourceText InvalidUserNameMessage =
               resourceMan.GetResourceText(SourceName, "InvalidUserNameMessage");
        internal static readonly ResourceText InvalidUserNameOrPasswordMessage =
               resourceMan.GetResourceText(SourceName, "InvalidUserNameOrPasswordMessage");
        internal static readonly ResourceText InvalidUserNameOrPasswordSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "InvalidUserNameOrPasswordSuggestionMessage");
        internal static readonly ResourceText TargetComputerNotRespondingMessage =
               resourceMan.GetResourceText(SourceName, "TargetComputerNotRespondingMessage");
        internal static readonly ResourceText TargetComputerNotRespondingSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "TargetComputerNotRespondingSuggestionMessage");
        internal static readonly ResourceText TrustedRelationshipFailureMessage =
               resourceMan.GetResourceText(SourceName, "TrustedRelationshipFailureMessage");
        internal static readonly ResourceText AgentIsAlreadyInstalledAndServerDiffersMessage =
               resourceMan.GetResourceText(SourceName, "AgentIsAlreadyInstalledAndServerDiffersMessage");
        internal static readonly ResourceText AgentIsAlreadyInstalledAndServerDiffersSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "AgentIsAlreadyInstalledAndServerDiffersSuggestionMessage");
        internal static readonly ResourceText AgentIsAlreadyInstalledMessage =
               resourceMan.GetResourceText(SourceName, "AgentIsAlreadyInstalledMessage");
        internal static readonly ResourceText AgentIsNotInstalledMessage =
               resourceMan.GetResourceText(SourceName, "AgentIsNotInstalledMessage");
        internal static readonly ResourceText FatalInstallationErrorMessage =
               resourceMan.GetResourceText(SourceName, "FatalInstallationErrorMessage");
        internal static readonly ResourceText HostNotAccessibleMessage =
               resourceMan.GetResourceText(SourceName, "HostNotAccessibleMessage");
        internal static readonly ResourceText HostNotAccessibleSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "HostNotAccessibleSuggestionMessage");
        internal static readonly ResourceText CannotStartRemoteServiceMessage =
               resourceMan.GetResourceText(SourceName, "CannotStartRemoteServiceMessage");
        internal static readonly ResourceText CannotSetupDescentServiceSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "CannotSetupDescentServiceSuggestionMessage");
        internal static readonly ResourceText CannotSetupDescentServiceMessage =
               resourceMan.GetResourceText(SourceName, "CannotSetupDescentServiceMessage");
        internal static readonly ResourceText CannotRemoveRemoteServiceMessage =
               resourceMan.GetResourceText(SourceName, "CannotRemoveRemoteServiceMessage");
        internal static readonly ResourceText CannotCopyFileMessage =
               resourceMan.GetResourceText(SourceName, "CannotCopyFileMessage");
        internal static readonly ResourceText CannotCopyFileSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "CannotCopyFileSuggestionMessage");
        internal static readonly ResourceText AnotherInstallationInProgressSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "AnotherInstallationInProgressSuggestionMessage");
        internal static readonly ResourceText AnotherInstallationInProgressMessage =
               resourceMan.GetResourceText(SourceName, "AnotherInstallationInProgressMessage");
        internal static readonly ResourceText AgentDescentTimeoutMessage =
               resourceMan.GetResourceText(SourceName, "AgentDescentTimeoutMessage");
        internal static readonly ResourceText AgentDescentTimeoutSuggestionMessage =
               resourceMan.GetResourceText(SourceName, "AgentDescentTimeoutSuggestionMessage");
        internal static readonly ResourceText NoCompatibleMsiFoundMessage =
               resourceMan.GetResourceText(SourceName, "NoCompatibleMsiFoundMessage");
        internal static readonly ResourceText ProductAlreadyInstalledMessage =
               resourceMan.GetResourceText(SourceName, "ProductAlreadyInstalledMessage");
        internal static readonly ResourceText UnsupportedErrorMessage =
               resourceMan.GetResourceText(SourceName, "UnsupportedErrorMessage");
        internal static readonly ResourceText InvalidResponseMessage =
               resourceMan.GetResourceText(SourceName, "InvalidResponseMessage");
        internal static readonly ResourceText DescentStatusDescriptionWithSuggestionFormat =
               resourceMan.GetResourceText(SourceName, "DescentStatusDescriptionWithSuggestionFormat");
        internal static readonly ResourceText ErrorCodeMessageFormat =
               resourceMan.GetResourceText(SourceName, "ErrorCodeMessageFormat");
        internal static readonly ResourceText DescentInstallAgentFailedFormat =
               resourceMan.GetResourceText(SourceName, "DescentInstallAgentFailedFormat");
        internal static readonly ResourceText DescentUninstallAgentFailedFormat =
               resourceMan.GetResourceText(SourceName, "DescentUninstallAgentFailedFormat");
        internal static readonly ResourceText DescentInstallAgentFailedTinyFormat =
               resourceMan.GetResourceText(SourceName, "DescentInstallAgentFailedTinyFormat");
        internal static readonly ResourceText DescentUninstallAgentFailedTinyFormat =
               resourceMan.GetResourceText(SourceName, "DescentUninstallAgentFailedTinyFormat");
        internal static readonly ResourceText DescentQueryAgentInfoFailedFormat =
               resourceMan.GetResourceText(SourceName, "DescentQueryAgentInfoFailedFormat");
        internal static readonly ResourceText DescentTimeoutSystemMessage =
               resourceMan.GetResourceText(SourceName, "DescentTimeoutSystemMessage");
        internal static readonly ResourceText PackageDescentTimeoutMessage =
               resourceMan.GetResourceText(SourceName, "PackageDescentTimeoutMessage");
        internal static readonly ResourceText TargetComputerNotManagedMessage =
               resourceMan.GetResourceText(SourceName, "TargetComputerNotManagedMessage");
        internal static readonly ResourceText DescentPackageCompletedRebootRequired =
                resourceMan.GetResourceText(SourceName, "DescentPackageCompletedRebootRequired");
        internal static readonly ResourceText DescentPackageCompletedRebootingInProgress =
                resourceMan.GetResourceText(SourceName, "DescentPackageCompletedRebootingInProgress");
        internal static readonly ResourceText DescentPackageCompleted =
                resourceMan.GetResourceText(SourceName, "DescentPackageCompleted");
        internal static readonly ResourceText DescentProgressAgentInstallConnectingToTarget =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallConnectingToTarget");
        internal static readonly ResourceText DescentProgressAgentInstallCopyingHelperServicePackage =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallCopyingHelperServicePackage");
        internal static readonly ResourceText DescentProgressAgentInstallConfiguringHelperService =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallConfiguringHelperService");
        internal static readonly ResourceText DescentProgressAgentInstallStartingHelperService =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallStartingHelperService");
        internal static readonly ResourceText DescentProgressAgentInstallSearchingExistingInstallations =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallSearchingExistingInstallations");
        internal static readonly ResourceText DescentProgressAgentInstallCopyingAgentPackage =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallCopyingAgentPackage");
        internal static readonly ResourceText DescentProgressAgentInstallInstallingAgentPackage =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallInstallingAgentPackage");
        internal static readonly ResourceText DescentProgressAgentInstallFinalizingAgentInstallation =
                resourceMan.GetResourceText(SourceName, "DescentProgressAgentInstallFinalizingAgentInstallation");
        internal static readonly ResourceText DescentUninstallIncompatibleUnsupportedMessage =
                resourceMan.GetResourceText(SourceName, "DescentUninstallIncompatibleUnsupportedMessage");
        internal static readonly ResourceText DescentUninstallIncompatibleUnsupportedSuggestionMessage =
                resourceMan.GetResourceText(SourceName, "DescentUninstallIncompatibleUnsupportedSuggestionMessage");
        internal static readonly ResourceText DescentUninstallIncompatiblePartiallyUnsupportedMessage =
                resourceMan.GetResourceText(SourceName, "DescentUninstallIncompatiblePartiallyUnsupportedMessage");
        internal static readonly ResourceText DescentUninstallIncompatiblePartiallyUnsupportedSuggestionMessage =
                resourceMan.GetResourceText(SourceName, "DescentUninstallIncompatiblePartiallyUnsupportedSuggestionMessage");
        internal static readonly ResourceText DescentOutdatedCisUninstallDiscoveryMessage =
                resourceMan.GetResourceText(SourceName, "DescentOutdatedCisUninstallDiscoveryMessage");
        internal static readonly ResourceText DescentOutdatedCisUninstallRemovingMessage =
                resourceMan.GetResourceText(SourceName, "DescentOutdatedCisUninstallRemovingMessage");
        internal static readonly ResourceText DescentAgentWaitingConnectionMessage =
                resourceMan.GetResourceText(SourceName, "DescentAgentWaitingConnectionMessage");
        internal static readonly ResourceText DescentUninstallIncompatibleDiscoveryMessage =
                resourceMan.GetResourceText(SourceName, "DescentUninstallIncompatibleDiscoveryMessage");

        internal static readonly ResourceText DescentUninstallOutdatedPackageMessage =
                resourceMan.GetResourceText(SourceName, "DescentUninstallOutdatedPackageMessage");
        internal static readonly ResourceText DescentInstallPackageMessage =
                resourceMan.GetResourceText(SourceName, "DescentInstallPackageMessage");

        internal static readonly ResourceText InternetAccessDescriptionFormatMessage =
                resourceMan.GetResourceText(SourceName, "InternetAccessDescriptionFormatMessage");

        internal static readonly ResourceText CisRequiresRebootingAfterScanMessage =
                resourceMan.GetResourceText(SourceName, "CisRequiresRebootingAfterScanMessage");

        internal static readonly ResourceText ScanTerminatedUnexpectedlyMessage =
                resourceMan.GetResourceText(SourceName, "ScanTerminatedUnexpectedlyMessage");

        internal static readonly ResourceText DescentFormatDescriptionWithoutSuggestion =
                resourceMan.GetResourceText(SourceName, "DescentFormatDescriptionWithoutSuggestion");

        internal static string GetString(string name)
        {
            return resourceMan.GetString(name);
        }
        internal static string GetString(string name, CultureInfo cultureInfo)
        {
            return resourceMan.GetString(name, cultureInfo);
        }
    }
}
