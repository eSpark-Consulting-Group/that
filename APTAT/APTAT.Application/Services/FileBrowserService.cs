using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Application.Models;

namespace APTAT.Application.Services
{
    public class FileBrowserService : IFileBrowserService
    {
        public void GetAvailableDrives(Object userState)
        {
            Task.Factory.StartNew(() =>
            {
                FolderItem[] drives = new FolderItem[0];
                Exception e = null;
                try
                {
                    var drivesInternal = DriveInfo.GetDrives();


                    drives =
                        drivesInternal.Where(d => d.DriveType == DriveType.Fixed)
                            .Select(d => new FolderItem(d.Name, false))
                            .ToArray();
                }
                catch (Exception ex)
                {
                    
                    e = ex;
                }
                
                App.Current.Dispatcher.Invoke(() =>
                        GetAvailableDrivesCompleted?.Invoke(this, new GetFoldersEventArgs(null, drives, userState, e)));
            });
        }

        public void GetFolders(FolderItem folderItem, Object userState)
        {
            Task.Factory.StartNew( ()=> 
            {
                FolderItem[] folders = new FolderItem[0];
                Exception e = null;
                try
                {
                    var foldersInternal = Directory.GetDirectories(folderItem.FullName);
                    folders = foldersInternal.Select(d => new FolderItem(d, true)).ToArray();
                }
                catch (Exception ex)
                {

                    e = ex;
                }

                App.Current.Dispatcher.Invoke(() =>
                        GetFoldersCompleted?.Invoke(this, new GetFoldersEventArgs(folderItem, folders, userState, e)));
            });
        }

        public event EventHandler<GetFoldersEventArgs> GetAvailableDrivesCompleted;
        public event EventHandler<GetFoldersEventArgs> GetFoldersCompleted;
    }
}
