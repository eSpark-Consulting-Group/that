using System.ComponentModel;

namespace Valkyrie.WebApiProvider.Enums
{
    public enum ValkyrieVerdict
    {
        /// <summary>
        /// means clean and its source is either Trusted Application or Manual Analysis
        /// </summary>
        Clean = -1,

        /// <summary>
        /// means undetected and its source is either Automatic Analysis or Manual Analysis.
        /// </summary>
        [Description("No Threat Found")]
        Undetected = 0,

        /// <summary>
        /// means malware and its source is either manual analysis or automatic analysis.
        /// </summary>        
        Malware = 1,

        /// <summary>
        ///means file doesn't exist.
        /// </summary> 
        [Description("Pending")]
        FirstSeen = 2,

        [Description("Potentially Unwanted Application")]
        PotentiallyUnwantedApplication = 3,

        [Description("Not Analysed")]
        NotAnalyzed = 101


    }
}