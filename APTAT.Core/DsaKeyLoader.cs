using System;
using APTAT.Core.Properties;

namespace APTAT.Core
{
    public static class DsaKeysLoader
    {
        private static byte[] _publicKey;
        private static byte[] _privateKey;

        public static void GetDsaKeys(out byte[] privateKey, out byte[] publicKey)
        {
            if (_publicKey == null || _privateKey == null)
            {
                string key = Settings.Default.DsaPrivateKey;
                _privateKey = Convert.FromBase64String(key);
                key = Settings.Default.DsaPublicKey;
                _publicKey = Convert.FromBase64String(key);
            }

            publicKey = _publicKey;
            privateKey = _privateKey;
        }
    }
}
