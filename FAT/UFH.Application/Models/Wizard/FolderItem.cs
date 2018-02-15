using System;
using System.Collections.ObjectModel;
using System.IO;
using UFH.Application.Models.ModelBase;

namespace UFH.Application.Models
{
    public class FolderItem : DomainObject, ISelectableTreeItem<FolderItem>
    {
        private ObservableCollection<FolderItem> _items;
        private readonly string _fullName;

        public FolderItem(string fullName, bool isFolder)
        {
            IsFolder = isFolder;
            _fullName = fullName;
            Name = isFolder ? Path.GetFileName(FullName) : _fullName;
        }

        public bool? IsSelected { get; set; }

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

        public override string ToString()
        {
            return FullName;
        }
    }
}
