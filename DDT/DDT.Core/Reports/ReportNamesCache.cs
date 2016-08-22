using System;
using System.Collections.Generic;
using System.Linq;

namespace DDT.Core.Reports
{
    public sealed class ReportNamesCache
    {
        private static readonly ReportNamesCache instance = new ReportNamesCache();
        static ReportNamesCache() { }
        private ReportNamesCache() { }
        public Dictionary<string, string> Reports = new Dictionary<string, string>();
        public static ReportNamesCache Instance
        {
            get { return instance; }
        }
    }
}
