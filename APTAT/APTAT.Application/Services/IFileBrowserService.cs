using APTAT.Application.Models;

namespace APTAT.Application.Services
{
    public interface IFileBrowserService
    {
        FolderItem[] GetAvailableDrives();
        FolderItem[] GetFolders(FolderItem folderItem);
    }
}