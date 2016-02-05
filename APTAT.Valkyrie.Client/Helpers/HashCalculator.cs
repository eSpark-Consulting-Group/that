using System;
using System.IO;
using System.Security.Cryptography;

namespace Valkyrie.WebApiProvider.Helpers
{
    public static class HashCalculator
    {
        public static string ComputeSHA1Hash(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read,FileShare.Read))
            {
                using (var sha1gen = new SHA1CryptoServiceProvider())
                {
                    sha1gen.ComputeHash(stream);
                    return BitConverter.ToString(sha1gen.Hash).Replace("-", "").ToLower();
                }
            }
        }
    }
}
