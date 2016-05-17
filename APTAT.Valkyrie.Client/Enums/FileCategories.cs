using System.ComponentModel;

namespace Valkyrie.WebApiProvider.Enums
{
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
}