using System;

namespace DDT.Application.Services
{
    public class DomainEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
    }
}