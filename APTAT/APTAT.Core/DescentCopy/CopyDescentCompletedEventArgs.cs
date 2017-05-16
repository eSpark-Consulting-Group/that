using APTAT.Core.Valkyrie;
using System;

namespace APTAT.Core.DescentCopy
{
    public class CopyDescentCompletedEventArgs : EventArgs
    {
        public IFileInfo[] Results { get; }

        public string ErrorMessage { get; }

        public CopyDescentCompletedEventArgs (IFileInfo[] results, string errorMessage)
        {
            Results = results;
            ErrorMessage = errorMessage;
        }

        public CopyDescentCompletedEventArgs(IFileInfo[] results)
        {
            Results = results;
        }
    }
}
