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
    public class FolderItem : DomainObject, ISelectableItem
    {
        private ObservableCollection<FolderItem> _items;
        private readonly string _fullName;

        public FolderItem(string fullName, bool isFolder)
        {
            IsFolder = isFolder;
            _fullName = fullName;
            Name = isFolder ? Path.GetFileName(FullName) : _fullName;
        }

        public Boolean IsSelected { get; set; }

        public string FullName
        {
            get
            {
                return IsFolder? _fullName: _fullName.TrimEnd('\\');
            }
        }

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
