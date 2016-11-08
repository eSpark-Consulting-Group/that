using System;
using System.Linq;

namespace APTAT.Core.FLS
{
    public class FlsResponse
    {
        public string Sha1 { get; private set; }
        public Int32 ClientIdNumber { get; private set; }
        public Byte NumberOfAnswers { get; private set; }

        public FlsVerdict FlsVerdict { get; private set; }
        public byte CamasVerdict { get; private set; }

        public FlsResponse(byte[] bytes, string hash)
        {
            if (bytes.Length < 7) throw new InvalidOperationException("Response size should be equal or more  7 bytes");
            var numBytes = bytes.Take(count: 4).ToArray();
            ClientIdNumber = BitConverter.ToInt32(numBytes, 0);
            NumberOfAnswers = bytes[4];
            FlsVerdict = (FlsVerdict) bytes[5];
            CamasVerdict = bytes[6];
            Sha1 = hash;
        }
    }
}