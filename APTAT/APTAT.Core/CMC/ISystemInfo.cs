namespace APTAT.Core.CMC
{
    public interface ISystemInfo
    {
        string FamilyName { get; }
        string Version    { get; }
        string Platform   { get; }
    }
}