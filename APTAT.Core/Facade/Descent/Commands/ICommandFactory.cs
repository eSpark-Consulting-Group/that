namespace APTAT.Core.Facade.Descent.Commands
{
	public interface ICommandFactory
	{
		/// <summary>
		/// Creates responseless command by request object type.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		IServiceCommand CreateCommand( object request );

		/// <summary>
		/// Creates request-response command by request object type.
		/// </summary>
		/// <typeparam name="TResponse"></typeparam>
		/// <param name="request"></param>
		/// <returns></returns>
		IServiceCommand<TResponse> CreateCommand<TResponse>( object request ) where TResponse : new();

		/// <summary>
		/// Creates requestless command by response type.
		/// </summary>
		/// <typeparam name="TResponse"></typeparam>
		/// <returns></returns>
		IServiceCommand<TResponse> CreateCommand<TResponse>() where TResponse : new();
	}
}
