using APTAT.Valkyrie.Client.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.Client.Contracts
{

    public class FullInfoResponse
    {
        [JsonProperty("additional_file_info")]
        public AdditionalFileInfo AdditionalFileInfo { get; set; }

        [JsonProperty("basic_file_info")]
        public BasicFileInfo BasicFileInfo { get; set; }

        [JsonProperty("behavioral_info")]
        public BehavioralInfo BehavioralInfo { get; set; }

        [JsonProperty("component_analysis_results")]
        public ComponentAnalysisResults ComponentAnalysisResults { get; set; }

        [JsonProperty("first_analysis_date")]
        public DateTime? FirstAnalysisDate { get; set; }

        [JsonProperty("last_analysis_date")]
        public DateTime? LastAnalysisDate { get; set; }

        [JsonProperty("malware_probability")]
        public float MalwareProbability { get; set; }

        [JsonProperty("number_of_seen_before")]
        public int NumberOfSeenBefore { get; set; }

        [JsonProperty("presense")]
        public bool Presence { get; set; }

        [JsonProperty("return_code")]
        public int ReturnCode { get; set; }

        [JsonProperty("sha1")]
        public string SHA1 { get; set; }

        [JsonProperty("submit_token")]
        public string SubmitToken { get; set; }

        [JsonProperty("verdict")]
        public InfoVerdictEnum Verdict { get; set; }

        [JsonProperty("verdict_source")]
        public string VerdictSource { get; set; }

        [JsonProperty("white_listed")]
        public bool WhiteListed { get; set; }
    }

    
    public class AdditionalFileInfo
    {
        public string compilation_time_stamp { get; set; }
        public string entry_point { get; set; }
        public int number_of_sections { get; set; }
        public Pe_Imports[] pe_imports { get; set; }
        public string[] pe_resources { get; set; }
        public string[][] pe_sections { get; set; }
        public string target_machine { get; set; }
    }

    public class Pe_Imports
    {
        public string[] imports { get; set; }
        public string name { get; set; }
    }

    public class BasicFileInfo
    {
        public int file_size { get; set; }
        public string file_type { get; set; }
        public string magic_literal { get; set; }
        public string md5 { get; set; }
        public string sha1 { get; set; }
        public string sha256 { get; set; }
    }

    public class BehavioralInfo
    {
        public string[] CreateFile { get; set; }
        public string[] CreateMutex { get; set; }
        public string[] CreateRegisteryKey { get; set; }
        public string[] LoadLibrary { get; set; }
        public string[] LowerChar { get; set; }
        public string[] OpenRegisteryKey { get; set; }
        public string[] QueryFilePath { get; set; }
        public string[] QueryProcessAddress { get; set; }
        public string[] ReadRegisteryKey { get; set; }
        public string[] ipAddresses { get; set; }
    }

    public class ComponentAnalysisResults
    {
        public Anomalies anomalies { get; set; }
        public Camas camas { get; set; }
        public string end_date { get; set; }
        public Fs fs { get; set; }
        public string md5 { get; set; }
        public Ml ml { get; set; }
        public int return_code { get; set; }
        public string session { get; set; }
        public string sha1 { get; set; }
        public string start_date { get; set; }
    }

    public class Anomalies
    {
        public object[] Antidebugcalls { get; set; }
        public int Antivmpresent { get; set; }
        public int Basedonthesectionsentropycheckfileispossiblypacked { get; set; }
        public int Enrtypointisoutsidethe1stcodesectionBinaryispossiblypacked { get; set; }
        public int HeaderChecksumiszero { get; set; }
        public int IllegalsizeofoptionalHeader { get; set; }
        public int Nonasciioremptysectionnamesdetected { get; set; }
        public int OptionalHeaderLoaderFlagsfieldisvaluedillegal { get; set; }
        public int OptionalHeaderNumberOfRvaAndSizesfieldisvaluedillegal { get; set; }
        public object[] Packerdetectiononsignaturedatabase { get; set; }
        public object[] Suspicousapicalls { get; set; }
        public int TLScallbackfunctionsarraydetected { get; set; }
        public int TheSizeOfRawdataisvaluedillegalBinarymightcrashyourdisassemblerdebugger { get; set; }
        public int Timestampvaluesuspicious { get; set; }
    }

    public class Camas
    {
        public string detailed_log { get; set; }
        public string log { get; set; }
        public float malware_probability { get; set; }
        public int return_code { get; set; }
        public int score { get; set; }
        public string session { get; set; }
        public string sha1 { get; set; }
        public float undetected_probability { get; set; }
        public int verdict { get; set; }
    }

    public class Fs
    {
        public float malware_probability { get; set; }
        public int return_code { get; set; }
        public string session { get; set; }
        public string sha1 { get; set; }
        public float undetected_probability { get; set; }
        public int verdict { get; set; }
        public string verdict_source { get; set; }
    }

    public class Ml
    {
        public float malware_probability { get; set; }
        public int return_code { get; set; }
        public string session { get; set; }
        public string sha1 { get; set; }
        public float undetected_probability { get; set; }
        public int verdict { get; set; }
    }

}
