using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core
{
    public class FlsResponse : IFlsResponse
    {
        private Int32 ClientIdNumber { get;  set; }
        private Byte NumberOfAnswers { get;  set; }

        public string[] GetFailedToAnalyzeHashes()
        {
            return Results.Where(r => r.IsNotAnalyzed()).Select(r=>r.Sha1).ToArray();
        }
        public IFlsResult[] GetAnalyzed()
        {
            return Results.Where(r => r.IsNotAnalyzed() == false).ToArray();
        }

        public List<IFlsResult> Results = new List<IFlsResult>();
        public FlsResponse(byte[] bytes, string[] hashes)
        {
            //if (bytes.Length < 7) throw new InvalidOperationException("Response size should be equal or more  7 bytes");
            byte[] numBytes = bytes.Take(4).ToArray();
            ClientIdNumber = BitConverter.ToInt32(numBytes, startIndex: 0);

            NumberOfAnswers = bytes[4];

            int byteIndex = 5;
            for (int i = 0; i < hashes.Length; i++)
            {
                FlsResult result = new FlsResult
                {
                    Cloud = ((FlsVerdict) bytes[byteIndex]).ToCloudVerdict(),
                 //   CamasVerdict = bytes[byteIndex + 1],
                    Sha1 = hashes[i]
                };
                Results.Add(result);
                byteIndex += 2;
            }
        }
    }
}