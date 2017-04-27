namespace APTAT.Core.Valkyrie
{
    public interface ICamRegistration
    {
        bool IsEmpty { get; }
        string LicenseId { get; }
        string Password { get; }
        string UserName { get; }
    }
}