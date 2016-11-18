namespace APTAT.Core
{
    public interface IFlsApiProvider
    {
        void Init();
        FlsResponse RequestType0(string[] hash);
        void UpdateFlsPorts(short udpPort, short tcpPort);
    }
}