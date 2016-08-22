using System;
using System.Runtime.Serialization;

namespace APTAT.DeploymentCore.Descent.Exceptions
{
    public class ScanCanceledException : Exception
    {
        public ScanCanceledException()
        {
            
        }

        public ScanCanceledException(string message)
            : base(message)
        {
            
        }

        public ScanCanceledException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }

        public ScanCanceledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }
    }
}
