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

        public CounterOrder OrderAsEnum => (CounterOrder) Order;

        public void UpdatePercentage(int total)
        {
            if (total > 0)
            {
                Percentage = string.Format("({0:0.00}%)",
                    Count * 100d / total).
                    Replace(".00", string.Empty).
                    Replace("0.00", string.Empty);
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

    public class ScanCounterManager
    {
        private readonly Dictionary<int, ScanCounter> _counters;
        private readonly ScanCounterCollection _collection;
        private Collection<int> _selected;
        private int _total;
        private static object _sync = new object();

        public Action<int[]> SelectedOrdersChanged { get; set; }

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
                HasClean = false;
                HasMalwares = false;
                HasUnknowns = false;
                HasNotAnalyzed = false;
                HasInProgress = false;
            }
        }

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
                    HasClean = counter.Valid;
                    break;
                case CounterOrder.Malware:
                    HasMalwares = counter.Valid;
                    break;
                case CounterOrder.Unknown:
                    HasUnknowns = counter.Valid;
                    break;
                case CounterOrder.NotAnalyzed:
                    HasNotAnalyzed = counter.Valid;
                    break;
                case CounterOrder.InProgress:
                    HasInProgress = counter.Valid;
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

        public void AddComputer(int count = 1)
        {
            AddCounter<ComputersCounter>(count);
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

        public bool HasClean { get; private set; }

        public bool HasMalwares { get; private set; }

        public bool HasUnknowns { get; private set; }

        public bool HasNotAnalyzed { get; private set; }

        public bool HasInProgress { get; private set; }

        public void UpdateCounters(IScanCounterDiff scanCounterDiff)
        {
            _total += scanCounterDiff.CleanDiff;
            _total += scanCounterDiff.MalwareDiff;
            _total += scanCounterDiff.UnknownDiff;
            _total += scanCounterDiff.NotAnalyzedDiff;
            _total += scanCounterDiff.InProgressDiff;

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