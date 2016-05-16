using System;
using Valkyrie.WebApiProvider.Contracts;

namespace APTAT.Core.Valkyrie
{
    public class ValkyrieRegistrationFailedException : Exception
    {
        public RegisterWithResponse Response { get; private set; }

        public override string Message
        {
            get
            {
                return Response != null && Response.ReturnCode != 0 ? Response.ResultDescription
                    : "Could not connect to Valkyrie Server.";
            }
        }

        public ValkyrieRegistrationFailedException(RegisterWithResponse response)
        {
            Response = response;
        }
    }
}
