using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace FAT.Application.Models
{
    public class ScanCounterDiff : IScanCounterDiff
    {
        public int UnknownFilesDiff { get; set; }
        public int MaliciousFilesDiff { get; set; }
        public int InProgressFilesDiff { get; set; }
        public int CleanFilesDiff { get; set; }
        public int NotAnalyzedFilesDiff { get; set; }
    }

    public enum CounterOrder
    {
        Computer,
        Clean,
        InProgress,
        Malware,
        NotAnalyzed,
        Unknown
    }

    public abstract class ScanCounter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Action<ScanCounter> SelectionChanged { get; set; }

        private int _count;
        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                if (!Valid && IsSelected)
                    IsSelected = false;
                RaisePropertyChanged(nameof(Count));
                RaisePropertyChanged(nameof(Valid));
            }
        }

        public bool Valid
        {
            get { return Count > 0; }
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
                    SelectionChanged?.Invoke(this);
                }
            }
        }

        public virtual int Order
        {
            get { return -1; }
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

        public void AddCollectionChanged(ScanCounter counter)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, counter));
        }
    }

    public class ScanCounterManager
    {
        private readonly Dictionary<int, ScanCounter> _counters;
        private readonly ScanCounterCollection _collection;
        private static object _sync = new object();

        public Action SelectionChanged { get; set; }

        public ScanCounterManager()
        {
            _counters = new Dictionary<int, ScanCounter>();
            _collection = new ScanCounterCollection();
            _collection.ExternalEnumerator = () => GetCounters;
        }

        public ScanCounterCollection Collection
        {
            get { return _collection; }
        }

        public void Clear()
        {
            lock (_sync)
            {
                _counters.Clear();
            }
        }

        private void AddCounter<T>(int count) where T : ScanCounter, new()
        {
            lock (_sync)
            {
                var counter = new T { Count = count, SelectionChanged = _ => SelectionChanged?.Invoke() };
                if (_counters.ContainsKey(counter.Order))
                    _counters[counter.Order].Count += count;
                else if (count > 0)
                {
                    _counters[counter.Order] = counter;
                    _collection.AddCollectionChanged(counter);
                }
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

        public void AddClean(int count = 1)
        {
            AddCounter<CleanCounter>(count);
        }

        public void AddMalware(int count = 1)
        {
            AddCounter<MalwareCounter>(count);
        }

        public void AddUnknown(int count = 1)
        {
            AddCounter<UnknownCounter>(count);
        }

        public void AddFailed(int count = 1)
        {
            AddCounter<FailedCounter>(count);
        }

        public void AddPending(int count = 1)
        {
            AddCounter<PendingCounter>(count);
        }

        public void UpdateCounters(IScanCounterDiff scanCounterDiff)
        {
            lock (_sync)
            {
                ScanCounter item;
                if (_counters.TryGetValue((int) CounterOrder.Clean, out item))
                    item.Count += scanCounterDiff.CleanFilesDiff;
                if (_counters.TryGetValue((int)CounterOrder.InProgress, out item))
                    item.Count += scanCounterDiff.InProgressFilesDiff;
                if (_counters.TryGetValue((int)CounterOrder.Malware, out item))
                    item.Count += scanCounterDiff.MaliciousFilesDiff;
                if (_counters.TryGetValue((int)CounterOrder.NotAnalyzed, out item))
                    item.Count += scanCounterDiff.NotAnalyzedFilesDiff;
                if (_counters.TryGetValue((int)CounterOrder.Unknown, out item))
                    item.Count += scanCounterDiff.UnknownFilesDiff;
            }
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

    public class FailedCounter : ScanCounter
    {
        public override int Order
        {
            get { return (int)CounterOrder.NotAnalyzed; }
        }
    }
    public class PendingCounter : ScanCounter
    {
        public override int Order
        {
            get { return (int)CounterOrder.InProgress; }
        }
    }
}