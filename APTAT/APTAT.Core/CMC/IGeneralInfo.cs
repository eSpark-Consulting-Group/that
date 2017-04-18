using System.Collections.Generic;

namespace APTAT.Core.CMC
{
    public interface IGeneralInfo
    {
        string ApplicationIdentifier { get; set; }
        string MachineCode { get; set; }
        string GlobalMachineCode { get; set; }
        string ProductIdentifier { get; }
        List<Parameter> Values { get; set; }
        List<EventInfo> EventsInfo { get; set; }
    }
}