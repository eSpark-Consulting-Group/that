using System;

namespace APTAT.Application.Services.EventArgs
{
    public class ErrorEventArgs : System.EventArgs
    {
        public ErrorEventArgs(Exception error)
        {
            Error = error;
        }
        public Exception Error { get; set; }
    }
}
