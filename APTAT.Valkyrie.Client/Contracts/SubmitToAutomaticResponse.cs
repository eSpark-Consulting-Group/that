using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.Client.Contracts
{

    public class SubmitToAutomaticResponse
    {
        public AdditionalFileInfo additional_file_info { get; set; }
        public Basic_File_Info basic_file_info { get; set; }
        public Behavioral_Info behavioral_info { get; set; }
        public Component_Analysis_Results component_analysis_results { get; set; }
        public string first_analysis_date { get; set; }
        public string last_analysis_date { get; set; }
        public float malware_probability { get; set; }
        public int number_of_seen_before { get; set; }
        public int return_code { get; set; }
        public string sha1 { get; set; }
        public int verdict { get; set; }
        public string verdict_source { get; set; }
        public int white_listed { get; set; }
    }

}
