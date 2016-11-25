using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APTAT.Core
{
    public interface IFlsManager
    {
        event EventHandler<FlsRequestEventArgs> FlsAnalyzed;

        Task Enqueue(IEnumerable<string> hashes, bool startIfCancelled = false);

        void Cancel();
        
        void UpdateFlsPorts(int udpPort, int tcpPort);
    }
}