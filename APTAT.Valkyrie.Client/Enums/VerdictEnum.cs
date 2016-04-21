using System.ComponentModel;

namespace Valkyrie.WebApiProvider.Enums
{
    public enum SkippReasonEnum
    {
        Skipped,
        Cancelled,
        Failed
    }

    public enum FileCategories
    {
        [Description("File does not exist")]
        Absent = 0,
        Safe = 1,
        Malware = 2,
        Unknown = 3,
        Game = 100,
        PCUtility = 101,
        Adware = 102,
        [Description("Potentially Unwanted Program")]
        PUP = 103,
        ProgrammingTool = 104,
        Finance = 105,
        Utilities = 106,
        SoftwareTool = 107,
        NetworkingSoftware = 108,
        BusinessSoftware = 109,
        ManufacturingTools = 110,
        SocialNetworks = 111,
        P2PApplications = 112,
        MultimediaApplications = 113
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

        PotentiallyUnwantedApplication = 3,

        [Description("Not Analyzed")]
        NotAnalyzed = 101


    }
}