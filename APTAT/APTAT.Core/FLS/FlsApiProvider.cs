using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using APTAT.Core.Properties;
using APTAT.DeploymentCore.Properties;

namespace APTAT.Core
{
    public class FlsApiProvider: IFlsApiProvider
    {
        private const int ClientIdNumber = 27102016;
        private IPAddress _server;
        
        private short _port = 4448;
        private string _hostName = "fls.security.comodo.com";

        public void UpdateFlsPorts(short udpPort, short tcpPort)
        {
            if (tcpPort == 0)
            {
                _port = short.Parse(Settings.Default.FlsPort);
            }
            else
            {
                _port = tcpPort;
            }
        }

        public void Init()
        {
            _hostName = Settings.Default.FlsHost;

            IPHostEntry hostEntry = Dns.GetHostEntry(_hostName);
            if (!hostEntry.AddressList.Any()) throw new InvalidOperationException("Could not resolve host " + _hostName);
            _server = hostEntry.AddressList[0];
        }
        public FlsApiProvider()
        {
            
        }

      

        public FlsResponse RequestType0(string[] hashes)
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
            var data = BuildRequestType0(hashes);

            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            var resultLength = 5 + hashes.Length * 2;
            var result = new byte[resultLength];

            stream.Read(result, 0, result.Length);
            var response = new FlsResponse(result, hashes);

            stream.Close();
            client.Close();
            return response;
        }

        private static byte[] BuildRequestType0(string[] sha)
        {
            var request = new FlsRequest
            {
                ApplicationID = Applications.UFH,
                CallerType = CallerType.OnAccess,
                ClientIdNumber = ClientIdNumber,
                ContentHash = sha,
                NumberOfHashes = (Byte) sha.Length,
                Identifier = Guid.NewGuid()
            };

            return request.GetBytes();
        }
    }
}