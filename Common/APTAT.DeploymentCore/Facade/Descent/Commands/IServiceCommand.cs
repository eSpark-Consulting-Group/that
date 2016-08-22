namespace APTAT.DeploymentCore.Facade.Descent.Commands
{
	public interface IServiceCommand
	{
		void Execute();
	}

	public interface IServiceCommand<TResponse> : IServiceCommand
	{
		TResponse Response
		{
			get;
		}
	}
}
