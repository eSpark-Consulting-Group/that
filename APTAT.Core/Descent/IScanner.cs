using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Core.Descent
{
    public interface IScanner : IDisposable
    {
        IDescentQueryResult ValidateAndGetResult();
        DescentEventArgs GetDescentCompletedEventArgs(IDescentQueryResult result);
        void StartScan(IDescentQueryResult result);
        void AbortAllThread();
    }
}
