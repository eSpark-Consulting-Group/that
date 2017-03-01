using System;

namespace FAT.Application.Models
{
    public class ScanCounterFactory : IScanCounterFactory
    {
        public IScanCounter CreateCounterByCounterOrder(CounterOrder counterOrder)
        {
            switch (counterOrder)
            {
                case CounterOrder.Clean:
                    return new CleanCounter();
                case CounterOrder.Computer:
                    return new ComputersCounter();
                case CounterOrder.InProgress:
                    return new InProgressCounter();
                case CounterOrder.Malware:
                    return new MalwareCounter();
                case CounterOrder.NotAnalyzed:
                    return new NotAnalyzedCounter();
                case CounterOrder.Unknown:
                    return new UnknownCounter();
                default:
                    throw new ArgumentOutOfRangeException(nameof(counterOrder));
            }
        }
    }
}
