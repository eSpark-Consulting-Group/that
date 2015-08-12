using APTAT.Valkyrie.Client;
using APTAT.Valkyrie.Client.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var infoRequeast = new InfoRequest() { Client_id = "MY_CLIENT", SHA1 = "2a171a332e9beb640e2682404f04ee54" };
            BasicInfoResponse basicInfo = ValkyrieClient.GetBasicInfo(infoRequeast);                        
            FullInfoResponse  fullInfo = ValkyrieClient.GetFullInfo(infoRequeast);

            
            var aTest = TestSubmitToAutomatic(basicInfo.SubmitToken);

            if (aTest.return_code != Client.Enums.ErrorCodesEnum.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(aTest.return_code);
                Console.ForegroundColor = ConsoleColor.White;
            }


            var mTest = TestSubmitToManual(basicInfo.SubmitToken);

            if (mTest.ReturnCode!= Client.Enums.ErrorCodesEnum.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(mTest.ResultDescription);
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine("Press any key to close console");

            Console.ReadKey();
        }

        private static SubmitToAutomaticResponse TestSubmitToAutomatic(string submitToken)
        {
            string virusPath = @"C:\Vir\vir\Gunpack_MiniSamples\Yzpack\24.ex$";
            SubmitToAutomaticRequest req = new SubmitToAutomaticRequest();
            req.Client_id = "MY_CLIENT";
            req.SubmitToken = submitToken;
            req.FileData = File.ReadAllBytes(virusPath);            
            var response = ValkyrieClient.SubmitToAutomaticAnalysis(req);
            return response;
        }
        private static SubmitToManualResponse TestSubmitToManual(string submitToken)
        {
            string virusPath = @"C:\Vir\vir\Gunpack_MiniSamples\Yzpack\24.exe";
            SubmitToManualRequest req = new SubmitToManualRequest();
            req.Client_id = "MY_CLIENT";
            req.SubmitToken = submitToken;
            req.FileData = File.ReadAllBytes(virusPath);
            req.Priority = 3;
            var response = ValkyrieClient.SubmitToManualAnalysis(req);
            return response;
        }
    }
}
