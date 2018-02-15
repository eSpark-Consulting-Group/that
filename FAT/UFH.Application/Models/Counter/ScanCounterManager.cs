using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UFH.Application.Models
{
    public class ScanCounterManager : IScanCounterManager
    {
        private readonly Dictionary<int, IScanCounter> _counters;
        private readonly IScanCounterCollection _collection;
        private readonly IScanCounterFactory _scanCounterFactory;
        private Collection<int> _selected;
        private int _total;
        private static object _sync = new object();

        public Action<int[]> SelectedOrdersChanged { private get; set; }

        public ScanCounterManager(IScanCounterCollection scanCounterCollection,
            IScanCounterFactory scanCounterFactory)
        {
            _scanCounterFactory = scanCounterFactory;
            _counters = new Dictionary<int, IScanCounter>();
            _collection = scanCounterCollection;
            _collection.ExternalEnumerator = () => GetCounters;
            _selected = new Collection<int>();
        }

        public IScanCounterCollection CounterCollection
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

        private void AddCounter(CounterOrder counterOrder, int count)
        {
            int counterOrderInt = (int)counterOrder;
            lock (_sync)
            {
                if (!_counters.ContainsKey(counterOrderInt))
                {
                    var counter = _scanCounterFactory.CreateCounterByCounterOrder(counterOrder);
                    counter.SelectionChanged = SelectionChanged;
                    counter.CountChanged = CountChanged;
                    _counters.Add(counter.Order, counter);
                    _collection.AddToCollection(counter);
                }
                _counters[counterOrderInt].Count += count;
                _counters[counterOrderInt].UpdatePercentage(_total);
            }
        }

        private void SelectionChanged(IScanCounter counter)
        {
            if (counter.IsSelected)
                _selected.Add(counter.Order);
            else
                _selected.Remove(counter.Order);

            SelectedOrdersChanged?.Invoke(_selected.ToArray());
        }

        private void CountChanged(IScanCounter counter)
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

        private IEnumerable<IScanCounter> GetCounters
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
            AddCounter(CounterOrder.Computer, 1);
        }

        private void AddClean(int count = 1)
        {
            AddCounter(CounterOrder.Clean, count);
        }

        private void AddMalware(int count = 1)
        {
            AddCounter(CounterOrder.Malware, count);
        }

        private void AddUnknown(int count = 1)
        {
            AddCounter(CounterOrder.Unknown, count);
        }

        private void AddNotAnalyzed(int count = 1)
        {
            AddCounter(CounterOrder.NotAnalyzed, count);
        }

        private void AddInProgress(int count = 1)
        {
            AddCounter(CounterOrder.InProgress, count);
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
}
