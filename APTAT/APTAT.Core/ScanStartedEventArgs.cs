using System;
using System.Security;

namespace APTAT.Core
{
    public class ScanStartedEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public Guid OperationUid { get; set; }
        public bool RescanAttempt { get; set; }
        public object State { get; set; }

        public string CamLogin { get; set; }
        public SecureString CamPassword { get; set; }
        public string CamLicenceID { get; set; }
    }
}