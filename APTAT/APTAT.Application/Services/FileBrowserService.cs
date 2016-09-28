using System.IO;
using System.Linq;
using System.Threading.Tasks;
using APTAT.Application.Models;

namespace APTAT.Application.Services
{
    public class FileBrowserService : IFileBrowserService
    {
        public async Task<FolderItem[]> GetAvailableDrives()
        {
            FolderItem[] drives = new FolderItem[0];
            await Task.Run(() =>
            {
                var drivesInternal = DriveInfo.GetDrives();
                drives = drivesInternal.Where(d => d.DriveType == DriveType.Fixed).Select(d => new FolderItem(d.Name, false)).ToArray();
            });
            return drives;
        }

        public async Task<FolderItem[]> GetFolders(FolderItem folderItem)
        {
            FolderItem[] folders = new FolderItem[0];
            await Task.Run(() =>
            {
                var foldersInternal = Directory.GetDirectories(folderItem.FullName);
                folders = foldersInternal.Select(d => new FolderItem(d, true)).ToArray();
            });
            return folders;
        }
    }
}
