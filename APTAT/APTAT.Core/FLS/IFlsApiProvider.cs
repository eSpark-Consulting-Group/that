namespace APTAT.Core
{
    public interface IFlsApiProvider
    {
        void Init();
        FlsResponse RequestType0(string[] hash);
    }
}