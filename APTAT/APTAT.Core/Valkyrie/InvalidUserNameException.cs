using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core.Valkyrie
{
    public class InvalidUserNameException : Exception
    {
        public Exception OriginalException { get; }

        public InvalidUserNameException(Exception exception)
        {
            OriginalException = exception;
        }
    }
}
