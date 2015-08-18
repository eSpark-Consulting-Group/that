using APTAT.Valkyrie.Client;
using APTAT.Valkyrie.Client.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.ClientTest
{
    class Program
    {

        static void Main(string[] args)
        {

            string folderPath = @"C:\Program Files\LibreOffice 5\program";
            Console.WriteLine("Starting uploading files to Valkyrie...");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var valkyrie = new ValkyrieClient();
            valkyrie.InProgress += Valkyrie_InProgress;
            try
            {
                valkyrie.SubmitFolderToAutomatic("MY_CLIENT", folderPath);
            }
            catch (AggregateException ex)
            {

                foreach (var item in ex.InnerExceptions)
                {
                    Console.WriteLine(item.Message);
                }
            }
            

            sw.Stop();

            var result = valkyrie.AnalysisResults;
            Console.WriteLine(String.Format("Upload to Valkyrie completed. Uploaded files {0}. Skipped files {1}", result.Where(f => f.IsSkipped == false).Count(), result.Where(f => f.IsSkipped).Count()));
            Console.WriteLine("Total Time elapsed: {0}", sw.Elapsed);

            ShowResult(result);

            Console.WriteLine("\n\nPress any key to close");
            Console.ReadKey();
        }

        private static void ShowResult(System.Collections.Concurrent.ConcurrentBag<AnalysisResult> result)
        {
            foreach (var item in result)
            {
                Console.WriteLine("-----------------------------------------------------------------------------------------");
                if (item.IsSkipped)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(String.Format("{0} {1} {2}  {3}", item.SHA1, "Skipped", item.LastAnalysisDate, item.SkipReason));
                }
                else
                {
                    if (item.Verdict == Client.Enums.InfoVerdictEnum.Malware)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.WriteLine(String.Format("{0} {1} {2}  Whitelisted: {3}", item.SHA1, item.Verdict, item.LastAnalysisDate, item.IsWhiteListed));
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static void Valkyrie_InProgress(object sender, InProgressEventArgs e)
        {
            Console.WriteLine(String.Format("Processed {0,3} from {1,3}\t{2:P}", e.ProcessedFileCount, e.TotalFileCount, ((double)e.Percentage / 100)));
        }



        //private static SubmitToManualResponse TestSubmitToManual(string submitToken)
        //{
        //    string virusPath = @"C:\Vir\vir\Gunpack_MiniSamples\Yzpack\24.ex$";
        //    SubmitToManualRequest req = new SubmitToManualRequest();
        //    req.Client_id = "MY_CLIENT";
        //    req.SubmitToken = submitToken;
        //    req.FileData = File.ReadAllBytes(virusPath);
        //    req.Priority = 3;
        //    var response = ValkyrieClient.SubmitToManualAnalysis(req);
        //    return response;
        //}
    }
}
