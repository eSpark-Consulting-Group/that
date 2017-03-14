namespace APTAT.Core
{
    public interface IFlsApiProvider
    {
        //void Init();
        IFlsResponse RequestType0(string[] hash);
        void UpdateFlsPorts(int udpPort, int tcpPort);
    }
}