using System;
using System.Collections.ObjectModel;

namespace APTAT.Application.Models
{
    public class ThreatInfoItem : DomainObject
    {
        private Int32 _progress;

        public Boolean IsRootItem { get; set; }

        public Boolean IsOnline { get; set; }

        public String Name { get; set; }

        public String Size { get; set; }

        public String Type { get; set; }

        public Object Target { get; set; }

        public Int32 Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                RaisePropertyChanged("Progress");
            }
        }

        public ObservableCollection<ThreatInfoItem> Items { get; set; }
    }
}
