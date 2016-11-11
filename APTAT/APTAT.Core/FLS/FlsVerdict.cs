namespace APTAT.Core
{
    public enum FlsVerdict
    {
        DoesntExist = 0,
        Safe = 1,
        Malicious = 2,
        Unknown = 3,
        Error = 4 // if fail (means can't connect to CAMDPAMS)
    }
}