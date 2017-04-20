namespace APTAT.Core.CMC
{
    public class GeneralInfoParameters : IGeneralInfoParameters
    {
        public string ApplicationIdentifier { get; set; }
        public string MachineCode { get; set; }
        public string GlobalMachineCode { get; set; }
        public string ProductIdentifier { get; set; }
    }
}