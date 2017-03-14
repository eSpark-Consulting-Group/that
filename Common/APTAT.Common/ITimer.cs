using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Common
{
    public interface ITimer : IDisposable
    {
        event EventHandler TimeoutCallback;
        void SetInverval(TimeSpan period);
        bool Start(bool autoReset = true);
        void Stop();
    }
}
