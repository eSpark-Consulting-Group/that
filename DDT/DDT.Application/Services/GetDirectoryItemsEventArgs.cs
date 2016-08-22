using System;
using DDT.Application.Models;
using System.Collections.ObjectModel;

namespace DDT.Application.Services
{
    public class GetDirectoryItemsEventArgs : DomainEventArgs
    {
        public DirectoryItem[] Items { get; private set; }

        public GetDirectoryItemsEventArgs(DirectoryItem[] items, Exception error = null)
        {
            Items = items;
            Exception = error;
        }
    }
}