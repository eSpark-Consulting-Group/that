using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Application.Models.ModelBase;

namespace APTAT.Application.Models
{
    public class FolderItem : DomainObject
    {
        private ObservableCollection<FolderItem> _items;

        public FolderItem(string fullName, bool isFolder)
        {
            IsFolder = isFolder;
            FullName = fullName;
            Name = isFolder ? Path.GetFileName(FullName) : FullName;
        }

        public string FullName { get;  }
        public string Name { get; }

        public FolderItem Parent { get; set; }
        public Boolean IsFolder { get; }

        public ObservableCollection<FolderItem> Items
        {
            get { return _items; }
            set { _items = value; RaisePropertyChanged("Items"); }
        }
    }
}
