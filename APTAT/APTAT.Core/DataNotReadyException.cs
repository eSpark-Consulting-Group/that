using System;

namespace APTAT.Core
{
    public class DataNotReadyException : Exception
    {
        public override string Message
        {
            get
            {
                return "Data for APT report is not ready.";
            }
        }
    }
}
