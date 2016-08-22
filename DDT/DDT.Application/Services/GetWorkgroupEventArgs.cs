using System;
using DDT.Application.Models;

namespace DDT.Application.Services
{
    public class GetWorkgroupEventArgs : DomainEventArgs
    {
        public DirectoryItem[] Items { get; private set; }

        public GetWorkgroupEventArgs(DirectoryItem[] items, Exception error = null)
        {
            Items = items;
            Exception = error;
        }
    }
}