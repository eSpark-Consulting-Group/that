using System;
using System.Net;
using System.Net.Sockets;

namespace APTAT.Core.FLS
{
    public class FlsApiProvider
    {
        private const int ClientIdNumber = 27102016;
        private readonly IPAddress _server;
        private readonly short _port;

        public FlsApiProvider(IPAddress server, short port)
        {
            _port = port;
            _server = server;
        }
        public FlsResponse RequestType0(string hash)
        {
            var client = new TcpClient
            {
                Client =
                {
                    SendTimeout = 10000,
                    ReceiveTimeout = 10000
                }
            };
            client.Connect(_server, _port);
            var data = BuildRequestType0(hash);

            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            data = new byte[7];

            stream.Read(data, 0, data.Length);
            var response = new FlsResponse(data, hash);

            stream.Close();
            client.Close();

            return response;

        }

        private static byte[] BuildRequestType0(string sha)
        {
            var request = new FlsRequest
            {
                ApplicationID = Applications.CIS,
                CallerType = CallerType.OnAccess,
                ClientIdNumber = ClientIdNumber,
                ContentHash = sha,
                NumberOfHashes = 1,
                Identifier = Guid.NewGuid()
            };

            return request.GetBytes();
        }
    }
}