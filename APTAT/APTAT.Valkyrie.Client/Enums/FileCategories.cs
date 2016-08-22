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
        [Description("Programming Tool")]
        ProgrammingTool = 104,
        
        Finance = 105,
        
        Utilities = 106,
        
        [Description("Software Tool")]
        SoftwareTool = 107,
        
        [Description("Networking Software")]        
        NetworkingSoftware = 108,

        [Description("Business Software")]
        BusinessSoftware = 109,

        ManufacturingTools = 110,
        SocialNetworks = 111,
        P2PApplications = 112,
        MultimediaApplications = 113
    }
}