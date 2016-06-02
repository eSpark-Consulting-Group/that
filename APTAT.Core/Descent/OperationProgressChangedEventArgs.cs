using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Core.Descent
{
    public class OperationProgressChangedEventArgs : EventArgs
    {
        public double Percentage { get; set; }
        public Guid OperationUid { get; set; }
    }
}
