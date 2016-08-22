using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace APTAT.Core
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
