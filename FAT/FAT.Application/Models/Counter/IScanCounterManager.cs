using System;

namespace FAT.Application.Models
{
    public interface IScanCounterManager
    {
        IScanCounterCollection CounterCollection { get; }
        int Total { get; }
        bool HasClean { get; }
        bool HasMalwares { get; }
        bool HasUnknowns { get; }
        bool HasNotAnalyzed { get; }
        bool HasInProgress { get; }
        int MalwaresCount { get; }
        int UnknownsCount { get; }

        Action<int[]> SelectedOrdersChanged { set; }

        void AddComputer();
        void UpdateCounters(IScanCounterDiff scanCounterDiff);
        void Clear();
    }
}
