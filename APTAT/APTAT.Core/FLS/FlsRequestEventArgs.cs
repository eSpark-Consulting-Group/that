using System;

namespace APTAT.Core
{
    public class FlsRequestEventArgs : EventArgs
    {
        public FlsResult[] Results { get; private set; }

        public Exception Exception { get; private set; }
        public FlsRequestEventArgs(FlsResult[] results, Exception exception)
        {
            Results = results;
            Exception = exception;
        }
    }
}