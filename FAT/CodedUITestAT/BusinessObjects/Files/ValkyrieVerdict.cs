using System.ComponentModel;

namespace CodedUITestAT
{
    /// <summary> Possible threat types for Valkyrie. </summary>
    internal enum ValkyrieVerdict 
    { 
      Malware,
      [DescriptionAttribute("In Analysis")]
      InAnalysis,
      [DescriptionAttribute("No Threat Found")]
      NoThreatFound,
      Clean
    };
}
