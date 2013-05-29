using System;
using System.IO;
using System.Security.Cryptography;

namespace APTAT.Core
{
    public class ProductSubSystem
    {
        public static bool VerifyHash(string expectedHash, string path)
        {
            if (!File.Exists(path))
                return false;
            using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return String.Equals(ComputeHash(file), expectedHash, StringComparison.OrdinalIgnoreCase);
            }
        }

        public static string ComputeHash(string path)
        {
            using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ComputeHash(file);
            }
        }

        public static string ComputeHash(Stream stream)
        {
            if (stream == null)
                return null;

            try
            {
                byte[] hash = new SHA1CryptoServiceProvider().ComputeHash(stream);
                string computed = BitConverter.ToString(hash).Replace("-", String.Empty);
                return computed;
            }
            finally
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
