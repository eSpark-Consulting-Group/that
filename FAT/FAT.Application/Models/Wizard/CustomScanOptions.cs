using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAT.Application.Models.Apt
{
    public class CustomScanOptions
    {
        public CustomScanOptions(FolderItem[] folderItems, bool scanCriticalAreas)
        {
            FolderItems = folderItems;
            ScanCriticalAreas = scanCriticalAreas;
        }
        public FolderItem[] FolderItems { get;  }
        public Boolean ScanCriticalAreas { get;  }
    }
}
