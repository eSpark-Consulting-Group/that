using System.Collections.Generic;

namespace APTAT.Core.CMC
{
    public interface IGeneralInfo
    {
        string ApplicationIdentifier { get; set; }
        string MachineCode { get; set; }
        string GlobalMachineCode { get; set; }
        string ProductIdentifier { get; set; }
        List<IParameter> Values { get; set; }
        List<IEventInfo> EventsInfo { get; set; }
    }
}