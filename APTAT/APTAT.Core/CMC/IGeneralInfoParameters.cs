namespace APTAT.Core.CMC
{
    public interface IGeneralInfoParameters
    {
        string ApplicationIdentifier { get;  }
        string MachineCode { get; }
        string GlobalMachineCode { get; }
        string ProductIdentifier { get; }
    }
}