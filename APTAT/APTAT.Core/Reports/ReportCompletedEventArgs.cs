using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Core.Reports
{
    public class ReportCompletedEventArgs : EventArgs
    {
        public object State { get; set; }
        public string FileName { get; set; }
        public Exception Error { get; set; }
    }
}
