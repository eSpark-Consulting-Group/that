using System;

namespace APTAT.Core.Descent
{
    public class StatusFileEmptyException : Exception
    {
        public override string Message
        {
            get
            {
                return "Status file is empty";
            }
        }
    }

    public class ResponseFileHeaderInvalidException : Exception
    {
        public override string Message
        {
            get
            {
                return "Response file header is not valid";
            }
        }
    }
}
