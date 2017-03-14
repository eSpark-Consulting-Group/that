using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Common
{
    public interface IScheduler : IDisposable
    {
        event EventHandler TimeoutCallback;
        void SetInterval(TimeSpan period);
        bool Start(bool autoReset = true);
        void Stop();
        bool IsEnabled { get; }
    }
}
