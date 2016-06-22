using System;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core
{
    public class ScanResultsProcessor
    {
        public bool TryParse(ref AptScanResult result)
        {
            if (result == null)
                return false;

            if (String.IsNullOrEmpty(result.FileName))
                return false;

            try
            {
                using (FileStream fs = new FileStream(result.FileName, FileMode.Open))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(AptReport));
                    result.SetAptReportInstance(xs.Deserialize(fs) as AptReport);
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
