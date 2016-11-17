using System;

namespace APTAT.Core
{
    public class FlsRequestEventArgs : EventArgs
    {
        public IFlsResult[] Results { get; private set; }

        public Exception Exception { get; private set; }
        public FlsRequestEventArgs(IFlsResult[] results, Exception exception)
        {
            Results = results;
            Exception = exception;
        }
    }
}