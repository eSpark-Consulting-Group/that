using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using APTAT.Core.Properties;
using APTAT.DeploymentCore.Properties;

namespace APTAT.Core
{
    public class FlsApiProvider : IFlsApiProvider
    {
        private const int ClientIdNumber = 27102016;
        private IPAddress _server;

        private int _port = 4448;
        private string _hostName = "fls.security.comodo.com";
        private int _udpPort;

        public void UpdateFlsPorts(int udpPort, int tcpPort)
        {
            if (tcpPort == 0)
            {
                _port = int.Parse(Settings.Default.FlsPort);
            }
            else
            {
                _port = tcpPort;
                _udpPort = udpPort;
            }
        }

        public void Init()
        {
            _hostName = Settings.Default.FlsHost;

            IPHostEntry hostEntry = Dns.GetHostEntry(_hostName);
            if (!hostEntry.AddressList.Any())
                throw new InvalidOperationException("Could not resolve host " + _hostName);
            _server = hostEntry.AddressList[0];
        }

        public FlsApiProvider()
        {

        }


        public FlsResponse RequestType0(string[] hashes)
        {
            FlsResponse result;
            try
            {
                result = RequestType0Udp(hashes);
            }
            catch (Exception ex)
            {
                result = RequestType0Tcp(hashes);
            }

            return result;
        }

        public FlsResponse RequestType0Udp(string[] hashes)
        {
            IPEndPoint ipep = new IPEndPoint(_server, _udpPort);
            UdpClient newsock = new UdpClient();
            newsock.Client.ReceiveTimeout = 10000;
            newsock.Client.SendTimeout = 10000;
            IPEndPoint sender = new IPEndPoint(_server, port: 0);
            newsock.Connect(ipep);
            var requestBytes = BuildRequestType0(hashes);
            newsock.Send(requestBytes, requestBytes.Length);

            var readBytes = newsock.Receive(ref sender);
            FlsResponse flsResponse = new FlsResponse(readBytes, hashes);

            return flsResponse;
        }

        public FlsResponse RequestType0Tcp(string[] hashes)
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