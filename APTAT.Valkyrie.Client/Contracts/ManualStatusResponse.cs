using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.Client.Contracts
{

    public class ManualStatusResponse
    {
        public object analysis_detail { get; set; }
        public object analysis_result_attachment_link { get; set; }
        public string analysis_status { get; set; }
        public string analysis_verdict { get; set; }
        public string manual_analysis_id { get; set; }
        public int return_code { get; set; }
        public string sha1 { get; set; }
    }

}
