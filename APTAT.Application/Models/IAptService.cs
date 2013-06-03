using System;

namespace APTAT.Application.Models
{
    public class GetDirectoryItemsEventArgs : EventArgs
    {
        public DirectoryItem Parent { get; private set; }

        public DirectoryItem[] Items { get; private set; }

        public Object UserState { get; private set; }

        public String Error { get; private set; }

        public GetDirectoryItemsEventArgs(DirectoryItem parent, DirectoryItem[] items, Object userState, String error = null)
        {
            Parent = parent;
            Items = items;
            UserState = userState;
            Error = error;
        }
    }

    public interface IAptService
    {
        event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryItemsCompleted;
        event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryRootCompleted;

        void  GetDirectoryItems(String domainController, String admin, String password, DirectoryItem parent, Object userState);

        void GetDirectoryRoot(String domainController, String admin, String password, Object userState);
    }
}
