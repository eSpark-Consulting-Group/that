using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class SubmitToAutomaticRequest: RequestBase
    {
        public string submit_token { get; set; }
        public string file_data { get; set; }
    }
}
