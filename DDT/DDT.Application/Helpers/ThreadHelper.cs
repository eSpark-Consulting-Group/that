using System;
using System.Threading;
using System.Windows.Threading;

namespace DDT.Application.Helpers
{

    public class ThreadHelper
    {
        private object _sync = new object();

        private Thread _thread;

        public static ThreadHelper Instance { get; private set; }

        ThreadHelper()
        {
        }

        static ThreadHelper()
        {
            Instance = new ThreadHelper();
        }

        private Dispatcher Dispatcher
        {
            get
            {
                if (_thread == null)
                {
                    _thread = new Thread(OnStart);
                    _thread.Name = "NewThread";
                    _thread.SetApartmentState(ApartmentState.STA);
                    _thread.IsBackground = true;

                    lock (_sync)
                    {
                        _thread.Start();
                        Monitor.Wait(_sync);
                    }
                }

                return Dispatcher.FromThread(_thread);
            }
        }

        public void DoEvents(Dispatcher dispatcher)
        {
            dispatcher.Invoke(delegate { }, DispatcherPriority.Background);
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

        private void OnStart(object arg)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke((Action)DispatcherStarted);
            Dispatcher.Run();
        }

        private void DispatcherStarted()
        {
            lock (_sync)
            {
                Monitor.Pulse(_sync);
            }
        }
    }
}
