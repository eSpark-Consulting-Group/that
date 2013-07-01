using System;

namespace APTAT.Core.Descent
{
    class AnotherScanIsInProgressException : Exception
    {
        public override string Message
        {
            get
            {
                return "Another scan is in progress.";
            }
        }
    }
}
