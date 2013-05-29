using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using APTAT.Core;

namespace APTAT.Core
{
    /// <summary>
    /// This utility interface help router to
    /// associate IP-identified andpoints with 
    /// corresponding IComputer objects.
    /// </summary>
    public interface IDirectoryResolver : IService
    {
        /// <summary>
        /// Resolves the specified ip to Computer object.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <returns>Computer object/</returns>
        IComputer Resolve(IPAddress ip);

        /// <summary>
        /// Resolves the specified MAC address to Computer object.
        /// </summary>
        /// <param name="macAddress">MAC address</param>
        /// <returns>Computer object/</returns>
        IComputer Resolve(PhysicalAddressSerial macAddress);

        /// <summary>
        /// Resolves the specified uid to Computer object.
        /// </summary>
        /// <param name="uid">The uid.</param>
        /// <returns>Computer object</returns>
        IComputer Resolve(Guid uid);

        /// <summary>
        /// Resolves the specified Sid to Computer object.
        /// </summary>
        /// <param name="sid">The Sid.</param>
        /// <returns>Computer object</returns>
        IComputer ResolveBySid(string sid);


        /// <summary>
        /// Resolves the specified DnsName to Computer object.
        /// </summary>
        /// <param name="name">The Dns name.</param>
        /// <returns>Computer object</returns>
        IComputer ResolveByDnsName(string name);

    }

    [Serializable]
    public sealed class PhysicalAddressSerial : ISerializable
    {
        private PhysicalAddress _macAddress = null;

        //public PhysicalAddress MACAddress
        //{
        //    get { return _macAddress; }
        //    set { _macAddress = value; }
        //}

        public override bool Equals(System.Object obj)
        {
            return Equals((PhysicalAddressSerial)obj);
        }

        public bool Equals(PhysicalAddressSerial p)
        {
            // If parameter is null return false.
            if (p == null)
            {
                return false;
            }

            int objHash = p.GetHashCode();
            int thisHash = this.GetHashCode();
            return objHash == thisHash;
        }

        public byte[] GetAddressBytes()
        {
            return _macAddress.GetAddressBytes();
        }

        public override int GetHashCode()
        {
            int Hash = 0;
            byte[] Mac = _macAddress.GetAddressBytes();
            for (uint i = 0; i < Mac.Length; i++)
            {
                Hash = (Hash * 397) ^ Mac[i].GetHashCode();
            }
            return Hash;
        }

        public PhysicalAddressSerial(byte[] bytesMAC)
        {
            _macAddress = new PhysicalAddress(bytesMAC);
        }

        void Load<T>(SerializationInfo info, string name, ref T item)
        {
            item = (T)info.GetValue(name, typeof(T));
        }

        // --------------------- SERIALIZATION ---------------------------
        public PhysicalAddressSerial(SerializationInfo info, StreamingContext context)
        {
            byte[] MAC = null;
            Load(info, "MACAddress", ref MAC);
            _macAddress = new PhysicalAddress(MAC);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (_macAddress != null && (_macAddress.GetAddressBytes().Length > 0))
            {
                byte[] bytes = _macAddress.GetAddressBytes();
                if (bytes != null)
                    info.AddValue("MACAddress", bytes);
            }
        }

        public override string ToString()
        {
            return _macAddress != null ? _macAddress.ToString() : String.Empty;
        }
    }
}
