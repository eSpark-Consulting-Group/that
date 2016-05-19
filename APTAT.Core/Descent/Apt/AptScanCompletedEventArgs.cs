using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Core.Descent.Apt
{
    public class AptScanCompletedEventArgs : DescentEventArgs
    {
        public AptScanOperationContext Context { get; set; }
    }
}
