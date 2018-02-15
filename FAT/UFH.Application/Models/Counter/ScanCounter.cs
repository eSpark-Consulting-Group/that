using System;
using System.ComponentModel;

namespace UFH.Application.Models
{
    public enum CounterOrder
    {
        Computer,
        Clean,
        Malware,
        Unknown,
        NotAnalyzed,
        InProgress
    }

    public abstract class ScanCounter : IScanCounter
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Action<IScanCounter> SelectionChanged { private get; set; }
        public Action<IScanCounter> CountChanged { private get; set; }

        private int _count;
        public int Count
        {
            get { return _count; }
            set
            {
                if (_count != value)
                {
                    _count = value;
                    if (!Valid && IsSelected)
                        IsSelected = false;
                    RaisePropertyChanged(nameof(Count));
                    RaisePropertyChanged(nameof(Valid));
                    CountChanged?.Invoke(this);
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged(nameof(IsSelected));
                    SelectionChanged?.Invoke(this);
                }
            }
        }

        public string Percentage { get; private set; }

        public bool Valid
        {
            get { return Count > 0; }
        }

        public virtual int Order
        {
            get { return -1; }
        }

        public virtual bool IsEnabled
        {
            get { return true; }
        }

        public CounterOrder OrderAsEnum => (CounterOrder) Order;

        public void UpdatePercentage(int total)
        {
            if (total > 0)
            {
                Percentage = string.Format("({0:0.00}%)",
                    Count * 100d / total).
                    Replace(".00", string.Empty).
                    Replace("0.00", "0.01");
                RaisePropertyChanged(nameof(Percentage));
            }
        }

        protected virtual void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class ComputersCounter : ScanCounter
    {
        public override int Order
        {
            get { return (int)CounterOrder.Computer; }
        }

        public override bool IsEnabled
        {
            get { return false; }
        }
    }

    public class CleanCounter : ScanCounter
    {
        public override int Order
        {
            get { return (int)CounterOrder.Clean; }
        }

        public override string ToString()
        {
            return "CleanCounterListItem";
        }
    }

    public class MalwareCounter : ScanCounter
    {
        public override int Order
        {
            get { return (int)CounterOrder.Malware; }
        }
        public override string ToString()
        {
            return "MalwareCounterListItem";
        }
    }

    public class UnknownCounter : ScanCounter
    {
        public override int Order
        {
            get { return (int)CounterOrder.Unknown; }
        }

        public override string ToString()
        {
            return "UnknownCounterListItem";
        }
    }

    public class NotAnalyzedCounter : ScanCounter
    {
        public override int Order
        {
            get { return (int)CounterOrder.NotAnalyzed; }
        }

        public override string ToString()
        {
            return "NotAnalyzedCounterListItem";
        }
    }
    public class InProgressCounter : ScanCounter
    {
        public override int Order
        {
            get { return (int)CounterOrder.InProgress; }
        }

        public override string ToString()
        {
            return "InAnalysisCounterListItem";
        }
    }
}