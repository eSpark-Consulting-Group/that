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
            BasicInfoResponse result = ValkyrieClient.GetBasicInfo(infoRequeast);                        
            FullInfoResponse fullInfo = ValkyrieClient.GetFullInfo(infoRequeast);


             var mTest = TestSubmitToManual();
            
            //curl - F "file_data=@/path/to/virus_3.ex$" - F "client_id=MY_CLIENT" - F "submit_token=b190a47b0cbfe25385b0cc8d02cd702b01b123f9-6a203f2b-7914-4899-ba2f-9a3aa4eef07f" - F "priority=3" "https://10.100.130.204/fvs_submit_manual"
            Console.ReadKey();
        }

        private static SubmitToManualResponse TestSubmitToManual()
        {
            string virusPath = @"C:\Vir\vir\Gunpack_MiniSamples\Yzpack\24.exe";
            SubmitToManualRequest req = new SubmitToManualRequest();
            req.Client_id = "MY_CLIENT";
            req.SubmitToken = "b190a47b0cbfe25385b0cc8d02cd702b01b123f9-6a203f2b-7914-4899-ba2f-9a3aa4eef07f";
            req.FileData = File.ReadAllBytes(virusPath);
            req.Priority = 3;
            var response = ValkyrieClient.SubmitToManualAnalysis(req);
            return response;
        }
    }
}
