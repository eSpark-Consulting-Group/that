using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace FAT.Application.Models
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

    public abstract class ScanCounter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Action<ScanCounter> SelectionChanged { get; set; }
        public Action<ScanCounter> CountChanged { get; set; }

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

        public string Percentage { get; set; }

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

    public class ScanCounterCollection : IEnumerable<ScanCounter>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public Func<IEnumerable<ScanCounter>> ExternalEnumerator { get; set; }

        public IEnumerator<ScanCounter> GetEnumerator()
        {
            return ExternalEnumerator?.Invoke().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddToCollection(ScanCounter counter)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, counter));
        }

        public void ResetCollection()
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public interface IScanCounterManager
    {
        ScanCounterCollection CounterCollection { get; }
        int Total { get; }
        bool HasClean { get;  }
        bool HasMalwares { get;  }
        bool HasUnknowns { get; }
        bool HasNotAnalyzed { get; }
        bool HasInProgress { get; }
        Action<int[]> SelectedOrdersChanged { set; }

        void AddComputer();
        void UpdateCounters(IScanCounterDiff scanCounterDiff);
        void Clear();
    }

    public class ScanCounterManager : IScanCounterManager
    {
        private readonly Dictionary<int, ScanCounter> _counters;
        private readonly ScanCounterCollection _collection;
        private Collection<int> _selected;
        private int _total;
        private static object _sync = new object();

        public Action<int[]> SelectedOrdersChanged { private get; set; }

        public ScanCounterManager()
        {
            _counters = new Dictionary<int, ScanCounter>();
            _collection = new ScanCounterCollection();
            _collection.ExternalEnumerator = () => GetCounters;
            _selected = new Collection<int>();
        }

        public ScanCounterCollection CounterCollection
        {
            get { return _collection; }
        }

        public int Total
        {
            get { return _total; }
        }

        public bool HasClean => CleanCount > 0;
        public bool HasMalwares => MalwaresCount > 0;
        public bool HasUnknowns => UnknownsCount > 0;
        public bool HasNotAnalyzed => NotAnalyzedCount > 0;
        public bool HasInProgress => InProgressCount > 0;

        public int CleanCount { get; private set; }

        public int MalwaresCount { get; private set; }

        public int UnknownsCount { get; private set; }

        public int NotAnalyzedCount { get; private set; }

        public int InProgressCount { get; private set; }

        private void AddCounter<T>(int count) where T : ScanCounter, new()
        {
            var counter = new T
            {
                SelectionChanged = SelectionChanged,
                CountChanged = CountChanged
            };

            lock (_sync)
            {
                if (!_counters.ContainsKey(counter.Order))
                {
                    _counters.Add(counter.Order, counter);
                    _collection.AddToCollection(counter);
                }
                _counters[counter.Order].Count += count;
                _counters[counter.Order].UpdatePercentage(_total);
            }
        }

        private void SelectionChanged(ScanCounter counter)
        {
            if (counter.IsSelected)
                _selected.Add(counter.Order);
            else
                _selected.Remove(counter.Order);

            SelectedOrdersChanged?.Invoke(_selected.ToArray());
        }

        private void CountChanged(ScanCounter counter)
        {
            switch (counter.OrderAsEnum)
            {
                case CounterOrder.Clean:
                    CleanCount = counter.Count;
                    break;
                case CounterOrder.Malware:
                    MalwaresCount = counter.Count;
                    break;
                case CounterOrder.Unknown:
                    UnknownsCount = counter.Count;
                    break;
                case CounterOrder.NotAnalyzed:
                    NotAnalyzedCount = counter.Count;
                    break;
                case CounterOrder.InProgress:
                    InProgressCount = counter.Count;
                    break;
            }
        }

        private IEnumerable<ScanCounter> GetCounters
        {
            get
            {
                lock (_sync)
                {
                    foreach (var value in _counters.Values)
                        yield return value;
                }
            }
        }

        public void AddComputer()
        {
            AddCounter<ComputersCounter>(1);
        }

        private void AddClean(int count = 1)
        {
            AddCounter<CleanCounter>(count);
        }

        private void AddMalware(int count = 1)
        {
            AddCounter<MalwareCounter>(count);
        }

        private void AddUnknown(int count = 1)
        {
            AddCounter<UnknownCounter>(count);
        }

        private void AddNotAnalyzed(int count = 1)
        {
            AddCounter<NotAnalyzedCounter>(count);
        }

        private void AddInProgress(int count = 1)
        {
            AddCounter<InProgressCounter>(count);
        }

        public void Clear()
        {
            lock (_sync)
            {
                _counters.Clear();
                _collection.ResetCollection();
                _total = 0;
                if (_selected.Any())
                {
                    _selected.Clear();
                    SelectedOrdersChanged?.Invoke(null);
                }
                CleanCount = 0;
                MalwaresCount = 0;
                UnknownsCount = 0;
                NotAnalyzedCount = 0;
                InProgressCount = 0;
            }
        }

        public void UpdateCounters(IScanCounterDiff scanCounterDiff)
        {
            lock (_sync)
            {
                _total += scanCounterDiff.CleanDiff;
                _total += scanCounterDiff.MalwareDiff;
                _total += scanCounterDiff.UnknownDiff;
                _total += scanCounterDiff.NotAnalyzedDiff;
                _total += scanCounterDiff.InProgressDiff;
            }

            AddClean(scanCounterDiff.CleanDiff);
            AddMalware(scanCounterDiff.MalwareDiff);
            AddUnknown(scanCounterDiff.UnknownDiff);
            AddNotAnalyzed(scanCounterDiff.NotAnalyzedDiff);
            AddInProgress(scanCounterDiff.InProgressDiff);
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
    }

    public class MalwareCounter : ScanCounter
    {
        public override int Order
        {
            get { return (int)CounterOrder.Malware; }
        }
    }

    public class UnknownCounter : ScanCounter
    {
        public override int Order
        {
            get { return (int)CounterOrder.Unknown; }
        }
    }

    public class NotAnalyzedCounter : ScanCounter
    {
        public override int Order
        {
            get { return (int)CounterOrder.NotAnalyzed; }
        }
    }
    public class InProgressCounter : ScanCounter
    {
        public override int Order
        {
            get { return (int)CounterOrder.InProgress; }
        }
    }
}