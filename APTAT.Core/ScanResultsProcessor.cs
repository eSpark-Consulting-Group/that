using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Diagnostics;

namespace APTAT.Core
{
    public class ScanResultsProcessor
    {
        public bool TryParse(ref ScanResult result)
        {
            if (result == null)
                return false;

            if (String.IsNullOrEmpty(result.FileName))
                return false;

            result.UnknownFiles = new List<UnknownFile>();

            try
            {
                XDocument xDocument = XDocument.Load(result.FileName);
                foreach (XElement element in xDocument.Descendants("File"))
                {
                    result.UnknownFiles.Add(new UnknownFile() { Name = element.Attribute("Name").Value, Size = Int64.Parse(element.Attribute("Size").Value) });
                }

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Trace(TraceEventType.Error, "Cannot parse the scan results file from '{0}({1})' endpoint: {2}", result.HostName, result.TargetUid, ex.Message);
                return false;
            }
        }
    }
}
