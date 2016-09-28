using System.Threading.Tasks;
using APTAT.Application.Models;

namespace APTAT.Application.Services
{
    public interface IFileBrowserService
    {
        Task<FolderItem[]> GetAvailableDrives();
        Task<FolderItem[]> GetFolders(FolderItem folderItem);
    }
}