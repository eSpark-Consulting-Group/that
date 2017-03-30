using System;
using System.Net;
using System.Net.Sockets;

namespace CodedUITestFAT
{
    /// <summary> Represents a computer. </summary>
    internal sealed class Computer : IRow
    {
        /// <summary> A network computer name. </summary>
        internal string Name { get { return _name; } }
        /// <summary> A computer row name at the application UI. </summary>
        public string RowName { get { return _rowName; } }
        /// <summary> A computer IP address. </summary>
        internal IPAddress IpAddress { get { return _ipAddress; } }

        /// <summary> Represents a local network computer. </summary>
        /// <param name="computerName">Set a computer isolated network name.</param>
        internal Computer(string computerName)
        {
            _name = computerName;            

            try
            {
                _ipAddress = Dns.GetHostAddresses(computerName)[0];
            }
            catch (SocketException) { _ipAddress = null; }

            _rowName = _name;
        }

        /// <summary> Represents a local network computer. </summary>
        /// <param name="computerName">Set a local computer IP address.</param>
        internal Computer(IPAddress ipAddress)
        {
            _ipAddress = ipAddress;

            try
            {
                var hostEntry = Dns.GetHostEntry(ipAddress);
                _name = hostEntry.HostName;
                _rowName = _name;
            }
            catch (SocketException)
            {
                _name = ipAddress.ToString();
                _rowName = _name;
            }
        }

        /// <summary> Represents a current computer. </summary>
        internal Computer()
        {
            _name = Environment.MachineName;            
            _ipAddress = SysHelper.GetLocalIpAddress();
            _rowName = _name;
        }

        private string _name;
        private string _rowName;
        private IPAddress _ipAddress;
    }
}
