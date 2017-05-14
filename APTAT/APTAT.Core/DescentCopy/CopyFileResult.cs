using APTAT.Core.Descent.Copy;
using APTAT.Core.Valkyrie;
using System;

namespace APTAT.Core.DescentCopy
{
    public class CopyFileResult : ICopyFileResult
    {
        private bool _disposed = false;
        public string RemoteFilePath { get; }
  
        public CopyFileResult(string remoteFilePath)
        {
            RemoteFilePath = remoteFilePath;
        }

        public void Dispose()
        {
            if (_disposed) return;
            try
            {
                System.IO.File.Delete(RemoteFilePath);
                _disposed = true;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
