using System;
using APTAT.Core.Facade.Descent.Commands;

namespace Esm.Server.Facade.Descent.Commands
{
	/// <summary>
	/// Service command without request and response.
	/// </summary>
	public abstract class ServiceCommand : IServiceCommand
	{
		public abstract void Execute();
	}

	/// <summary>
	/// Service command with request and without response.
	/// </summary>
	public abstract class RequestServiceCommand<TRequest> : ServiceCommand
	{
		public RequestServiceCommand( TRequest request )
		{
			if( request == null )
			{
				throw new ArgumentNullException( "request" );
			}

			Request = request;
		}

		public TRequest Request
		{
			get;
			private set;
		}
	}

	/// <summary>
	/// Service command without request and with response.
	/// </summary>
	public abstract class ResponseServiceCommand<TResponse> : ServiceCommand, IServiceCommand<TResponse> where TResponse : new()
	{
		public ResponseServiceCommand()
		{
			Response = new TResponse();
		}

		public TResponse Response
		{
			get;
			protected set;
		}
	}

	/// <summary>
	/// Service command with request and response.
	/// </summary>
	public abstract class ServiceCommand<TRequest, TResponse> : RequestServiceCommand<TRequest>, IServiceCommand<TResponse> where TResponse : new()
	{
		public ServiceCommand( TRequest request )
			: base( request )
		{
			
			Response = new TResponse();
		}

		public TResponse Response
		{
			get;
			protected set;
		}
	}
}
