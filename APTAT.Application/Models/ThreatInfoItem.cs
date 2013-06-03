using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace APTAT.Application.Models
{
    public class ThreatInfoItem : INotifyPropertyChanged
    {
        private Int32 _progress;

        public Boolean IsRootItem { get; set; }

        public Int64 Id { get; set; }

        public String Name { get; set; }

        public String Size { get; set; }

        public String Type { get; set; }

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

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(String name)
        {
            

            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
