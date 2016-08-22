using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace DDT.Core.NMap
{
    public class NmapScanner
    {
        public static XmlDocument DetectOSToXml(string target, bool quick)
        {
            using (var process = new Process())
            {
                var fileName = Path.GetTempFileName();
                var args = string.Format("-O --osscan-guess -oX {0} {1}", fileName, target);
                if (quick)
                {
                    args = string.Format("-p 22,80,445,65123,56123 -O -oX {0} {1}", fileName, target);
                }

                const string nmapPath = "nmap.exe";

                var startInfo = new ProcessStartInfo(nmapPath, args) { WindowStyle = ProcessWindowStyle.Hidden };
                process.StartInfo = startInfo;

                process.Start();
                process.WaitForExit();
                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);
                return doc;
            }
        }

        public static List<osmatch> DetectOS(string target, bool quick)
        {
            List<osmatch> result = new List<osmatch>();
            XmlDocument doc = DetectOSToXml(target, quick);
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/nmaprun/host/os/osmatch");

            foreach (XmlNode node in nodes)
            {
                osmatch n = node.OuterXml.ParseXML<osmatch>();
                result.Add(n);
            }

            return result;
        }
        public static void Scan(string target, string tempFile, string scanArguments)
        {
            var watch = Stopwatch.StartNew();


            using (var process = new Process())
            {
                var arguments = string.Format("{0} \"{1}\" {2}", scanArguments, tempFile, target);



                //TODO Resolve nmap.exe location or alert that during nmap installation "path" variable should be updated
                var nmapPath = "nmap.exe";

                var startInfo = new ProcessStartInfo(nmapPath, arguments) { WindowStyle = ProcessWindowStyle.Hidden };

                process.StartInfo = startInfo;

                process.Start();


                process.WaitForExit();


            }



            watch.Stop();

            var elapsedMs = watch.ElapsedMilliseconds;


        }
    }
}
