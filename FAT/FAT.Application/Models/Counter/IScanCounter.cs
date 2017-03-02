using System;
using System.ComponentModel;

namespace FAT.Application.Models
{
    public interface IScanCounter : INotifyPropertyChanged
    {
        int Count { get; set; }
        Action<IScanCounter> CountChanged { set; }
        Action<IScanCounter> SelectionChanged { set; }
        bool IsEnabled { get; }
        bool IsSelected { get; set; }
        int Order { get; }
        CounterOrder OrderAsEnum { get; }
        string Percentage { get; }
        bool Valid { get; }

        void UpdatePercentage(int total);
    }
}