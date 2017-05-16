using APTAT.Core.Descent.Copy;
using APTAT.Core.Valkyrie;
using System;

namespace APTAT.Core.DescentCopy
{
    public class CopyFileResult : ICopyFileResult
    {
        private bool _disposed = false;
        public string RemoteFilePath { get; }
        public int? Win32Error { get; }
        
        public CopyFileResult(string remoteFilePath)
        {
            RemoteFilePath = remoteFilePath;
        }

        public CopyFileResult(int win32Error)
        {
            Win32Error = win32Error;
        }

        public void Dispose()
        {
            if (_disposed) return;
            if (Win32Error == null) return;
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
