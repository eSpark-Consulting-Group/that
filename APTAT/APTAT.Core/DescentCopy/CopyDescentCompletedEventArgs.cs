using APTAT.Core.Valkyrie;
using System;

namespace APTAT.Core.DescentCopy
{
    public class CopyDescentCompletedEventArgs : EventArgs
    {
        public IFileInfo[] Results { get; }
        
        public CopyDescentCompletedEventArgs (IFileInfo[] results)
        {
            Results = results;
        }
    }
}
