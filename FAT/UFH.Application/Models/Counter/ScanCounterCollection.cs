using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace UFH.Application.Models
{
    public class ScanCounterCollection : IScanCounterCollection
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public Func<IEnumerable<IScanCounter>> ExternalEnumerator { private get; set; }

        public IEnumerator<IScanCounter> GetEnumerator()
        {
            return ExternalEnumerator?.Invoke().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddToCollection(IScanCounter counter)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, counter));
        }

        public void ResetCollection()
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}