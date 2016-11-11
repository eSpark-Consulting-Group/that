using System;
using System.Collections.Generic;
using System.Linq;

namespace APTAT.Core
{
    public class FlsRequest
    {
        public Byte RequestType { get; set; } //T
        public Int32 ClientIdNumber { get; set; } //XXXX
        public Byte NumberOfHashes { get; set; } //Z         
        public Byte HashType { get; set; } //H – hashes type, 0 for SHA1 (one byte)

        public Byte ProtocolVersion
        {
            get { return 3; }
        } //P 
        public Applications ApplicationID { get; set; } //A
        public Guid Identifier { get; set; } //GUID
        public CallerType CallerType { get; set; } //C        
        public string[] ContentHash { get; set; } //CONTENT_HASH

        public byte[] GetBytes()
        {
            List<byte> byteList = new List<byte>();
            byteList.Add(RequestType); //T
            byte[] clientID = BitConverter.GetBytes(ClientIdNumber); //XXXX
            byteList.AddRange(clientID);
            byteList.Add(NumberOfHashes); //Z
            byteList.Add(HashType); //H
            byteList.Add(ProtocolVersion); //P
            byteList.Add((Byte) ApplicationID); //A
            byteList.AddRange(Identifier.ToByteArray()); //Guid 16bytes
            byteList.Add((Byte) CallerType); //C

            foreach (var s in ContentHash)
            {
                var hashBytes = StringToByteArray(s); //CONTENT_HASH
                byteList.AddRange(hashBytes);
            }

            return byteList.ToArray();
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}