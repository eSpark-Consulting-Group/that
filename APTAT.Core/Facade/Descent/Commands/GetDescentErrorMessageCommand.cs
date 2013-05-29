using System;
using System.Globalization;
using APTAT.Core.Descent;
using APTAT.Core.Facade.Descent.MessageContracts;
using Esm.Server.Facade.Descent.Commands;

namespace APTAT.Core.Facade.Descent.Commands
{
	public class GetDescentErrorMessageCommand : ServiceCommand<GetDescentErrorMessageRequest, GetDescentErrorMessageResponse>
	{
        public GetDescentErrorMessageCommand(GetDescentErrorMessageRequest request)
			: base( request )
		{
		}

		public override void Execute()
		{
			var descentSubsystem = SubSystemFactory.Current.GetSubSystem<IDescentSubSystem>();
			try
			{
			    var cultureInfo = Request.CultureInfoName != null
                    ? new CultureInfo(Request.CultureInfoName)
			        : null;
			    Response.ErrorMessage = descentSubsystem.GetDescentErrorMessage(Request.ErrorCode, Request.SystemCode, Request.SystemMessage, Request.ExtensionTexts, false, cultureInfo);
			}
			catch( FormatException )
			{
				//throw new FaultException<DescentErrorCodeInvalidFault>( new DescentErrorCodeInvalidFault { ErrorCode = Request.ErrorCode }, "ErrorCode is invalid." );
                throw new InvalidOperationException("ErrorCode is invalid.");
			}
		}
	}
}
