using System;
using System.Collections.Generic;
using System.Linq;

namespace APTAT.Core
{
    public class FlsResponse
    {
        public Int32 ClientIdNumber { get; private set; }
        public Byte NumberOfAnswers { get; private set; }

        public string[] GetFailedToAnalyzeHashes()
        {
            return Results.Where(r => r.FlsVerdict == FlsVerdict.Error).Select(r=>r.Sha1).ToArray();
        }
        public FlsResult[] GetAnalyzed()
        {
            return Results.Where(r => r.FlsVerdict != FlsVerdict.Error).ToArray();
        }

        public List<FlsResult> Results = new List<FlsResult>();
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
                    FlsVerdict = (FlsVerdict) bytes[byteIndex],
                    CamasVerdict = bytes[byteIndex + 1],
                    Sha1 = hashes[i]
                };
                Results.Add(result);
                byteIndex += 2;
            }
        }
    }
}