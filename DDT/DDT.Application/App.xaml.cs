
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using APTAT.Common;
using DDT.Application.Helpers;

namespace DDT.Application
{
    public partial class App : System.Windows.Application
    {
        private EventWaitHandle _event;
        private SplashWindow _splash;
        private bool _initialized;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _event = new EventWaitHandle(false, EventResetMode.ManualReset, "Global\\F49126C7-111C-46E5-B790-62B7BD1FD858", out createdNew);
            
            if (!createdNew)
            {
                MessageBox.Show("Auto Discovery and Deployment Tool is already running.", "Auto Discovery and Deployment Tool");
                Current.Shutdown();
            }
            else
            {
                ThreadHelper.Instance.RunOnThread(delegate { (_splash = new SplashWindow()).Show(); });

                var op = ThreadHelper.Instance.RunOnThreadDelayed(delegate
                {
                    Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                    {
                        Source = new Uri("/DDT.Application;component/AppResources.xaml", UriKind.RelativeOrAbsolute)
                    });
                    CompositionHelper.Instance.RegisterModules(new[]
                {
                    typeof(APTAT.Common.DependencyLocator),
                    typeof(APTAT.DeploymentCore.DependencyLocator),
                    typeof(Core.DependencyLocator),
                    typeof(DependencyLocator)
                });
                });
                op.Completed += DispatcherOperationCompleted;

                var timer = new DispatcherTimer(DispatcherPriority.Normal, Current.Dispatcher);
                timer.Interval = TimeSpan.FromMilliseconds(2000);
                timer.Tick += TimerTick;
                timer.Start();

                op.Priority = DispatcherPriority.Normal;

                Current.DispatcherUnhandledException += AppDispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                InitTraceListener();
            }

            base.OnStartup(e);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (_initialized)
            {
                var timer = sender as DispatcherTimer;
                timer.Tick -= TimerTick;
                timer.Stop();

                var window = new MainWindow();
                window.Loaded += (s, a) => _splash.Dispatcher.Invoke(_splash.Close);
                window.Show();
                window.Activate();
            }
        }

        private void DispatcherOperationCompleted(object sender, EventArgs e)
        {
            var op = sender as DispatcherOperation;
            op.Completed -= DispatcherOperationCompleted;

            _initialized = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _event?.Dispose();

            base.OnExit(e);
        }

        private void InitTraceListener()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Comodo");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, "ADDT");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, "trace.log");
            Trace.Listeners.Add(new TextWriterTraceListener(path, "ADDTListener"));
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogHelper.Trace(TraceEventType.Error, e.ExceptionObject + (e.ExceptionObject is Exception && ((Exception)e.ExceptionObject).InnerException != null ? "\n InnerException : " + ((Exception)e.ExceptionObject).InnerException : null));
        }

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG
            e.Handled = false;
#else
            e.Handled = true;
            LogHelper.Trace(TraceEventType.Error, e.Exception + (e.Exception.InnerException != null ? "\n InnerException :" + (e.Exception).InnerException : null));
#endif
        }
    }
}