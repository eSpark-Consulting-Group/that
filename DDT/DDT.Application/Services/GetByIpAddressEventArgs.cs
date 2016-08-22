using System;
using DDT.Application.Models;

namespace DDT.Application.Services
{
    public class GetByIpAddressEventArgs : DomainEventArgs
    {
        public DeploymentItem[] Items { get; private set; }

        public GetByIpAddressEventArgs(DeploymentItem[] items, Exception error = null)
        {
            Items = items;
            Exception = error;
        }
    }
}