using APTAT.Valkyrie.Client;
using APTAT.Valkyrie.Client.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var basicInfoRequest = new BasicInfoRequest() { Client_id = "MY_CLIENT", SHA1 = "b190a47b0cbfe25385b0cc8d02cd702b01b123f9" };
            BasicInfoResponse result = ValkyrieClient.GetBasicInfo(basicInfoRequest);

            var fullInfoRequest = new FullInfoRequest { Client_id = "MY_CLIENT", SHA1 = "b190a47b0cbfe25385b0cc8d02cd702b01b123f9" };
            FullInfoResponse fullInfo = ValkyrieClient.GetFullInfo(fullInfoRequest);
            Console.ReadKey();
        }
    }
}
