using System;
using System.Threading;
using System.Windows.Threading;

namespace APTAT.Application.Helpers
{
    public class LoadingHelper
    {
        private readonly Thread _bThread;
        private readonly AutoResetEvent _bEvent = new AutoResetEvent(false);

        private Dispatcher _dispatcher;
        private Dispatcher Dispatcher
        {
            get
            {
                if (_dispatcher == null)
                {
                    _bEvent.WaitOne();

                    _dispatcher = Dispatcher.FromThread(_bThread);
                }
                return _dispatcher;
            }
        }

        public void DoEvents(Dispatcher dispatcher)
        {
            dispatcher.Invoke((Action)delegate { }, DispatcherPriority.Background);
        }

        public DispatcherOperation RunOnThread(Action act)
        {
            return Dispatcher.BeginInvoke(DispatcherPriority.Normal, act);
        }

        public DispatcherOperation RunOnThread<T>(Func<T> act)
        {
            return Dispatcher.BeginInvoke(DispatcherPriority.Normal, act);
        }

        public DispatcherOperation RunOnThreadDelayed<T>(Func<T> act, params object[] list)
        {
            return Dispatcher.BeginInvoke(act, DispatcherPriority.Inactive, list);
        }

        public DispatcherOperation RunOnThreadDelayed(Action act, params object[] list)
        {
            return Dispatcher.BeginInvoke(act, DispatcherPriority.Inactive, list);
        }

        public LoadingHelper()
        {
            _bThread = new Thread(OnStart);
            _bThread.Name = "NewThread";
            _bThread.SetApartmentState(ApartmentState.STA);
            _bThread.IsBackground = true;
            _bThread.Priority = ThreadPriority.Normal;
            _bThread.Start(null);
        }

        private void OnStart(object arg)
        {
            _bEvent.Set();
            Dispatcher.Run();
            _bEvent.Close();
        }
    }
}


