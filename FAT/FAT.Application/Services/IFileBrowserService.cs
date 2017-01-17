using System;
using System.Threading.Tasks;
using FAT.Application.Models;

namespace FAT.Application.Services
{
    public interface IFileBrowserService
    {
        event EventHandler<GetFoldersEventArgs> GetAvailableDrivesCompleted;
        event EventHandler<GetFoldersEventArgs> GetFoldersCompleted;
        void GetAvailableDrives(Object userState);
        void GetFolders(FolderItem folderItem, Object userState);
    }
    public class GetFoldersEventArgs : DomainEventArgs
    {
        public FolderItem Parent { get; private set; }

        public FolderItem[] Items { get; private set; }

        public GetFoldersEventArgs(FolderItem parent, FolderItem[] items, Object userState, Exception error = null)
        {
            Parent = parent;
            Items = items;
            UserState = userState;
            Exception = error;
        }
    }
}