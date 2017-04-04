using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace APTAT.Core.Reports
{
    internal sealed class ReportNamesCache : IReportNamesCache
    {
        //private static readonly ReportNamesCache instance = new ReportNamesCache();
        //static ReportNamesCache() { }
        //private ReportNamesCache() { }
        //public static ReportNamesCache Instance => instance;

        private Dictionary<string, string> Reports = new Dictionary<string, string>();

        public void Add(string reportKey, string fileName)
        {
            Reports.Add(reportKey, fileName);
        }
        public ReportFile TryGetReportFromCache(string reportKey)
        {
            var existentName = string.Empty;
            if (Reports.Keys.Contains(reportKey))
            {
                existentName = Reports[reportKey];
            }

            if (!string.IsNullOrWhiteSpace(existentName))
            {
                if (File.Exists(existentName))
                {
                    var data = File.ReadAllBytes(existentName);
                    return new ReportFile() { FileName = existentName, DocumentBytes = data };
                }
                else
                {
                    Reports.Remove(reportKey);
                }
            }
            return null;
        }
    }
}