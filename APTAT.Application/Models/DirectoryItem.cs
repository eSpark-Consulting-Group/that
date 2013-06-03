using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace APTAT.Application.Models
{
    public enum DirectoryItemType
    {
        Root,
        OrgUnit,
        Computer
    }

    public class DirectoryItem : DomainObject
    {
        private ObservableCollection<DirectoryItem> _items;

        public String LdapPath { get; set; }

        public String Name { get; set; }

        public String DnsName { get; set; }

        public Guid TargetUid { get; set; }

        public DirectoryItemType Type { get; set; }

        public ObservableCollection<DirectoryItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }
    }
}
