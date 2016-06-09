using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace APTAT.Valkyrie.ClientTest
{
    class Program
    {

          [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CsvReaderForm());

            //string folderPath = @"C:\Users\Yevhen.Bal\AppData\Local\Temp\ValkyrieTmp";
            //Console.WriteLine("Starting uploading files to Valkyrie...");
            ////Stopwatch sw = new Stopwatch();
            ////sw.Start();
            //var valkyrie = new ValkyrieClient("MY_CLIENT");
            //valkyrie.InProgress += Valkyrie_InProgress;
            //valkyrie.AnalyzeCanceled += valkyrie_AnalyzeCanceled;
            //valkyrie.AnalyzeCompleted += valkyrie_AnalyzeCompleted;
            //try
            //{
            //    valkyrie.AnalyzeAsync("MY_CLIENT", folderPath);

            //    for (int i = 0; i < 1000; i++)
            //    {
            //        //if (i == 5) valkyrie.AnalyzeAsyncCancel();
            //        //Console.WriteLine(i);
            //        Thread.Sleep(1000);
            //    }

            //    //valkyrie.SubmitFolderToAutomatic("MY_CLIENT", folderPath);


            //    //List<string> files = GetDuplicatedCollection(folderPath);

            //    //valkyrie.UploadFilesToValkyrie("MY_CLIENT", files);


            //}          
            //catch (AggregateException ex)
            //{

            //    foreach (var item in ex.InnerExceptions)
            //    {
            //        Console.WriteLine(item.Message);
            //    }
            //}
            
            
            ////sw.Stop();

            ////var result = valkyrie.AnalysisResults;
            ////Console.WriteLine(String.Format("Upload to Valkyrie completed. Uploaded files {0}. Skipped files {1}", result.Where(f => f.IsSkipped == false).Count(), result.Where(f => f.IsSkipped).Count()));
            ////Console.WriteLine("Total Time elapsed: {0}", sw.Elapsed);

            ////ShowResult(result);

            //Console.WriteLine("\n\nPress any key to close");
            //Console.ReadKey();
        }

        //static void valkyrie_AnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<ValkyrieAnalysisResult> e)
        //{
        //    Console.ForegroundColor = ConsoleColor.Green;
        //    var result = e.Result;
        //    Console.WriteLine(String.Format("Upload to Valkyrie completed. Uploaded files {0}. Skipped files {1}", result.Where(f => f.IsSkipped == false).Count(), result.Where(f => f.IsSkipped).Count()));
            

        //    ShowResult(result);
        //    Console.ForegroundColor = ConsoleColor.White;
        //}

        static void valkyrie_AnalyzeCanceled(object sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Valkyrie analysis canceled");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static List<string> GetDuplicatedCollection(string folderPath)
        {
            var files = Directory.EnumerateFiles(folderPath).ToList();
            var f = files.First();

            var duplicatedList = new List<String>();
            duplicatedList.Add(f);
            duplicatedList.Add(f);
            duplicatedList.Add(f);
            duplicatedList.Add(f);
            duplicatedList.Add(f);
            var collection1 = files.Take(5).ToList();
            files.AddRange(collection1);
            files.AddRange(duplicatedList);
            return files;
        }

        //private static void ShowResult(IEnumerable<ValkyrieAnalysisResult> result)
        //{
        //    foreach (var item in result)
        //    {
        //        Console.WriteLine("-----------------------------------------------------------------------------------------");
        //        if (item.IsSkipped)
        //        {
        //            Console.ForegroundColor = ConsoleColor.Yellow;
        //            Console.WriteLine("{0} {1} {2}  {3}", item.SHA1, "Skipped", item.LastAnalysisDate, item.SkipReason);
        //        }
        //        else
        //        {
        //            if (item.Verdict == Client.Enums.ValkyrieVerdict.Malware)
        //            {
        //                Console.ForegroundColor = ConsoleColor.Red;
        //            }
        //            else
        //            {
        //                Console.ForegroundColor = ConsoleColor.White;
        //            }
        //            Console.WriteLine(String.Format("{0} {1} {2}  Whitelisted: {3}", item.SHA1, item.Verdict, item.LastAnalysisDate, item.IsWhiteListed));
        //        }
        //        Console.ForegroundColor = ConsoleColor.White;
        //    }
        //}
               


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
