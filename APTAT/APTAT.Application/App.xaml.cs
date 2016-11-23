using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using APTAT.Common;
using APTAT.Application.Helpers;

namespace APTAT.Application
{
    public partial class App : System.Windows.Application
    {
        private Mutex _mutex;
        private SplashWindow _splash;
        private MainWindow _window;
        private DispatcherTimer _timer;
        private DispatcherOperation _operation;
        private bool _initialized;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _mutex = new Mutex(true, "Global\\8F24B2C3-13A4-490A-ACD5-0D68C7BAEA81", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("Unknown File Hunter is already running.", "Unknown File Hunter");
                Current.Shutdown();
            }
            else
            {
                Current.DispatcherUnhandledException += AppDispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                _operation = ThreadHelper.Instance.RunOnThread(delegate { (_splash = new SplashWindow()).Show(); });
                var op = ThreadHelper.Instance.RunOnThreadDelayed(delegate 
                {
                    Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                    {
                        Source = new Uri("/UnknownFileHunter;component/AppResources.xaml", UriKind.RelativeOrAbsolute)
                    });
                    CompositionHelper.Instance.RegisterModules(new[]
                    {
                        typeof(Common.DependencyLocator),
                        typeof(DeploymentCore.DependencyLocator),
                        typeof(DependencyLocator),
                        typeof(Valkyrie.DependencyLocator.DependencyLocator),
                        typeof(Core.DependencyLocator.DependencyLocator)
                    });
                });
                op.Completed += DispatcherOperationCompleted;

                _timer = new DispatcherTimer(DispatcherPriority.Normal, Current.Dispatcher);
                _timer.Interval = TimeSpan.FromMilliseconds(2000);
                _timer.Tick += TimerTick;
                _timer.Start();

                op.Priority = DispatcherPriority.Normal;

                InitTraceListener();
            }

            base.OnStartup(e);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (_initialized)
            {
                _timer.Tick -= TimerTick;
                _timer.Stop();
                
                _window = new MainWindow();
                _window.Loaded += WindowLoaded;
                _window.Show(); 
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _window.Loaded -= WindowLoaded;
            _window.Activate();
            _operation.Dispatcher.Invoke(_splash.Close);
        }

        private void DispatcherOperationCompleted(object sender, EventArgs e)
        {
            var op = sender as DispatcherOperation;
            op.Completed -= DispatcherOperationCompleted;

            _initialized = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.Close();

            base.OnExit(e);
        }

        private void InitTraceListener()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Comodo");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, "APTAT");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, "trace.log");
            Trace.Listeners.Add(new TextWriterTraceListener(path, "APTATListener"));
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
