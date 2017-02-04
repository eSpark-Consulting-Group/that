using System;
using APTAT.Core.Apt;

namespace APTAT.Core.FLS
{
    public class FlsRequestEventArgs<T> : EventArgs where T:IFlsResult
    {
        public T[] Results { get; private set; }

        public Exception Exception { get; private set; }
        public FlsRequestEventArgs(T[] results, Exception exception)
        {
            Results = results;
            Exception = exception;
        }
    }
}