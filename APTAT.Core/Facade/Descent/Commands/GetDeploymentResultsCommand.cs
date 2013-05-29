using System;
using System.Linq;
using APTAT.Core.Descent;
using APTAT.Core.Facade.Descent.DataContracts;
using APTAT.Core.Facade.Descent.MessageContracts;
using Esm.Server.Facade.Descent.Commands;

namespace APTAT.Core.Facade.Descent.Commands
{
	public class GetDeploymentResultsCommand : ServiceCommand<GetDeploymentResultsRequest, GetDeploymentResultsResponse>
	{
		public GetDeploymentResultsCommand( GetDeploymentResultsRequest request )
			: base( request )
		{
		}

		public override void Execute()
		{
			var descentSubsystem = SubSystemFactory.Current.GetSubSystem<IDescentSubSystem>();
			DescentResult result = descentSubsystem.GetDescentResult( Request.OperationUid );
			if( result == null )
			{
				//throw new FaultException<DescentOperationNotFoundFault>( new DescentOperationNotFoundFault { OperationUid = Request.OperationUid }, "Descent operation not found." );
                throw new InvalidOperationException("Descent operation not found.");
			}

			Response.Completed = result.Completed;
			var computerResults = (Request.StartFrom >= 0 && Request.Count > 0) ?
				result.ComputerResults.Skip(Request.StartFrom).Take(Request.Count) :
				result.ComputerResults;

			foreach( var computerRes in computerResults )
			{
				DeploymentStep step = computerRes.CurrentStage.ToFacade();
				//Console.ForegroundColor = ConsoleColor.Cyan;
				//System.Diagnostics.Debug.WriteLine(
				//    String.Format( "Computer result queried: {16}{0} computer name: {1}{0} deployment step: {15}{0} agent completed: {2}{0} agent succeeded: {3}{0} uninstall completed: {4}{0} uninstall succeeded: {5}{0} package completed: {6}{0} package succeeded: {7}{0} completed: {8}{0} succeeded: {9}{0} progress percentage: {10}{0} status message: {11}{0} error code: {12}{0} system message: {13}{0} system code: {14}{0}",
				//    Environment.NewLine,
				//    computerRes.Computer.Name,
				//    computerRes.AgentCompleted,
				//    computerRes.AgentSucceeded,
				//    computerRes.UninstallCompleted,
				//    computerRes.UninstallSucceeded,
				//    computerRes.PackageCompleted,
				//    computerRes.PackageSucceeded,
				//    computerRes.Completed,
				//    computerRes.Succeeded,
				//    computerRes.ProgressPercentage,
				//    computerRes.StatusMessage,
				//    computerRes.ErrorCode,
				//    computerRes.SystemMessage,
				//    computerRes.SystemCode,
				//    step,
				//    DateTime.Now ), "DSC" );
				//Console.ResetColor();

				DeploymentResult facadeResult = new DeploymentResult
				{
					Completed = computerRes.Completed,
					Step = step,
					Succeeded = computerRes.Succeeded,
					TargetUid = computerRes.TargetUid,
					ComputerName = computerRes.Computer.Name,
					IPAddress = computerRes.Computer.IPAddress.ToStringOrEmpty(),
					ErrorCode = computerRes.ErrorCode,
					SystemCode = computerRes.SystemCode,
					SystemMessage = computerRes.SystemMessage,
                    StatusMessage = String.IsNullOrEmpty(computerRes.ShortStatusMessage.ResourceId) ? computerRes.StatusMessage.ToStringOrEmpty() : computerRes.StatusMessage.ResourceId,
					Progress = computerRes.ProgressPercentage,
					ExtensionTexts = computerRes.ExtensionTexts,
					TotalProgress = computerRes.TotalProgressPercentage,
				};

				InitCompletedSteps( facadeResult, computerRes );

				Response.DeploymentResults.Add( facadeResult );
			}
		}

		private static void InitCompletedSteps( DeploymentResult result, ComputerDescentResult originalResult )
		{
			if( result.Completed && result.Succeeded )
			{
				result.CompletedSteps = new CompletedStepsCollection();
				switch( result.Step )
				{
					case DeploymentStep.AgentInstall:
						result.CompletedSteps.Add( new CompletedStep { Index = 0, Status = CompletionStatus.Succeeded, Step = DeploymentStep.AgentInstall } );
						break;
					case DeploymentStep.CisInstall:
						int index = 0;
						result.CompletedSteps.Add( new CompletedStep { Index = index, Status = CompletionStatus.Succeeded, Step = DeploymentStep.AgentInstall } );
						index++;
						if( originalResult.WithUninstall )
						{
							result.CompletedSteps.Add( new UninstallCompletedStep { Index = index, Status = CompletionStatus.Succeeded, Step = DeploymentStep.IncompatibleSoftwareUninstall, UninstalledSoftware = originalResult.UninstalledSoftware } );
							index++;
						}

						string[] uninstCis = String.IsNullOrEmpty( originalResult.OutdatedCis ) ? null : new string[] { originalResult.OutdatedCis };
						result.CompletedSteps.Add( new UninstallCompletedStep { Index = index, Status = CompletionStatus.Succeeded, Step = DeploymentStep.OutdatedCisUninstall, UninstalledSoftware = uninstCis } );
						result.CompletedSteps.Add( new CompletedStep { Index = index, Status = CompletionStatus.Succeeded, Step = DeploymentStep.CisInstall } );
						break;
				}
			}
		}
	}

    public static partial class MapperHelper
    {
        public static DeploymentStep ToFacade(this DescentStage stage)
        {
            switch (stage)
            {
                case DescentStage.AgentInstall:
                    return DeploymentStep.AgentInstall;
                case DescentStage.OutdatedCisUninstall:
                    return DeploymentStep.OutdatedCisUninstall;
                case DescentStage.PackageInstall:
                    return DeploymentStep.CisInstall;
                case DescentStage.IncompatibleUninstall:
                    return DeploymentStep.IncompatibleSoftwareUninstall;
                default:
                    throw new ArgumentOutOfRangeException("stage");
            }
        }

        public static string ToStringOrEmpty<T>(this T source) where T : class
        {
            return source != null ? source.ToString() : String.Empty;
        }
    }
}
