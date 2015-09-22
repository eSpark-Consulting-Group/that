using System.ComponentModel;
namespace APTAT.Valkyrie.Client.Enums
{
    public enum SkippReasonEnum
    {
        Skipped, 
        Cancelled, 
        Failed
    }

    public enum InfoVerdictEnum
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
        /// means verdict not ready and its source is Manual Analysis.
        /// </summary>
        NotReady = 100,

        
        [Description("Not uploaded")]
        NotUploadedToValkyrie = 101

        
    }
}