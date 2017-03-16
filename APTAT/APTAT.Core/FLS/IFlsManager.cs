using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using APTAT.Core.FLS;

namespace APTAT.Core
{
    public interface IFlsManager : IDisposable
    {
        event EventHandler<FlsRequestEventArgs<IFlsResult>> FlsAnalyzed;

        void Enqueue(IEnumerable<string> hashes);

        void Cancel();

        void UpdateFlsPorts(int udpPort, int tcpPort);
    }
}