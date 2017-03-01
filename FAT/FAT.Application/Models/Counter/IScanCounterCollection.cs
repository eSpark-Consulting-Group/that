using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace FAT.Application.Models
{
    public interface IScanCounterCollection : IEnumerable<IScanCounter>, INotifyCollectionChanged
    {
        Func<IEnumerable<IScanCounter>> ExternalEnumerator { set; }
        void AddToCollection(IScanCounter counter);
        void ResetCollection();
    }
}