using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Telerik.Windows.Controls;

namespace APTAT.Application.Helpers
{
    public struct LoadProgressWrapper
    {
        public HostVisual Host { get; set; }
        public VisualTargetPresentationSource ProcessControl { get; set; }
        public bool ElementCreated { get; set; }
    }

    public static class LoadingHelper
        {
            private static readonly Thread backgroundThread;
            private static readonly AutoResetEvent backgroundEvent = new AutoResetEvent(false);
            private static readonly Thread workingThread;
            private static readonly AutoResetEvent workingEvent = new AutoResetEvent(false);

            public static Dispatcher MainDispatcher { get; set; }

            private static Dispatcher _backgroundUIDispatcher;
            private static Dispatcher BackgroundUIDispatcher
            {
                get
                {
                    if (_backgroundUIDispatcher == null)
                    {
                        backgroundEvent.WaitOne();

                        _backgroundUIDispatcher = Dispatcher.FromThread(backgroundThread);
                    }
                    return _backgroundUIDispatcher;
                }
            }

            private static Dispatcher _workerDispatcher;
            private static Dispatcher WorkerDispatcher
            {
                get
                {
                    if (_workerDispatcher == null)
                    {
                        workingEvent.WaitOne();

                        _workerDispatcher = Dispatcher.FromThread(workingThread);
                    }
                    return _workerDispatcher;
                }
            }

            public static void DoEvents(Dispatcher dispatcher)
            {
                dispatcher.Invoke((Action)delegate { }, DispatcherPriority.Background);
            }

            public static DispatcherOperation RunOnWorkingThread(Action act)
            {
                return WorkerDispatcher.BeginInvoke(DispatcherPriority.Normal, act);
            }

            public static DispatcherOperation RunOnWorkingThread<T>(Func<T> act)
            {
                return WorkerDispatcher.BeginInvoke(DispatcherPriority.Normal, act);
            }

            public static DispatcherOperation RunOnWorkingThreadDelayed<T>(Func<T> act, params object[] list)
            {
                return WorkerDispatcher.BeginInvoke(act, DispatcherPriority.Inactive, list);
            }

            public static DispatcherOperation RunOnWorkingThreadDelayed(Action act, params object[] list)
            {
                return WorkerDispatcher.BeginInvoke(act, DispatcherPriority.Inactive, list);
            }


            public static void RunOnThread(Action act)
            {
                BackgroundUIDispatcher.Invoke(DispatcherPriority.Background, act);
            }

            public static void StartProgress(LoadProgressWrapper res)
            {
                if (res.ElementCreated)
                {
                    RunOnThread(delegate
                    {
                        ((Loading)res.ProcessControl.RootVisual).IsBusy = true;
                    });
                }
            }

            public static void StopProgress(LoadProgressWrapper res)
            {
                if (res.ElementCreated)
                {
                    RunOnThread(delegate
                    {
                        ((Loading)res.ProcessControl.RootVisual).IsBusy = false;
                    });
                }
            }

            static LoadingHelper()
            {
                backgroundThread = new Thread(BackgroundUIStart);
                backgroundThread.Name = "BackgroundUI";
                backgroundThread.SetApartmentState(ApartmentState.STA);
                backgroundThread.IsBackground = true;
                backgroundThread.Priority = ThreadPriority.Normal;
                backgroundThread.Start(null);

                workingThread = new Thread(WorkingStart);
                workingThread.Name = "WorkingThread";
                workingThread.SetApartmentState(ApartmentState.STA);
                workingThread.IsBackground = true;
                workingThread.Priority = ThreadPriority.Normal;
                workingThread.Start(null);
            }

            public static LoadProgressWrapper CreateElementOnWorkerThread<T>(params object[] args) where T : FrameworkElement
            {
                var hostVisual = new HostVisual();
                VisualTargetPresentationSource vps = null;

                while (BackgroundUIDispatcher == null)
                {
                }

                var dop = BackgroundUIDispatcher.BeginInvoke(
                (Action)delegate
                {
                    vps = new VisualTargetPresentationSource(hostVisual);
                    vps.RootVisual = CreateControl(typeof(T), args);
                }, DispatcherPriority.Normal, null);

                while (dop.Status == DispatcherOperationStatus.Executing || dop.Status == DispatcherOperationStatus.Pending)
                {
                    Thread.Sleep(0);
                }

                if (dop.Status != DispatcherOperationStatus.Completed) return new LoadProgressWrapper();

                return new LoadProgressWrapper { Host = hostVisual, ProcessControl = vps, ElementCreated = true };
            }

            private static FrameworkElement CreateControl(Type t, params object[] args)
            {
                return (FrameworkElement)Activator.CreateInstance(t, args);
            }

            private static void BackgroundUIStart(object arg)
            {
                backgroundEvent.Set();
                Dispatcher.Run();
                backgroundEvent.Close();
            }

            private static void WorkingStart(object arg)
            {
                workingEvent.Set();
                Dispatcher.Run();
                workingEvent.Close();
            }
        }
    }


