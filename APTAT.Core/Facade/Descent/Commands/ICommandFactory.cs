namespace APTAT.Core.Facade.Descent.Commands
{
	public interface ICommandFactory
	{

		/// <summary>
		/// Creates request-response command by request object type.
		/// </summary>
		/// <typeparam name="TResponse"></typeparam>
		/// <param name="request"></param>
		/// <returns></returns>
		IServiceCommand<TResponse> CreateCommand<TResponse>( object request ) where TResponse : new();
	}
}
