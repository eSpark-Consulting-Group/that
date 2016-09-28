using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Application.Models;

namespace APTAT.Application.Services
{
    public class FileBrowserService : IFileBrowserService
    {
        public FolderItem[] GetAvailableDrives()
        {
            var drives = DriveInfo.GetDrives();
            return drives.Where(d => d.DriveType == DriveType.Fixed).Select(d => new FolderItem(d.Name, false)).ToArray();
        }

        public FolderItem[] GetFolders(FolderItem folderItem)
        {
            var folders = Directory.GetDirectories(folderItem.FullName);
            return folders.Select(d => new FolderItem(d, true)).ToArray();
        }
    }
}
