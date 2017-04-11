using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using APTAT.Common;
using FAT.Application.Helpers;

namespace FAT.Application
{
    public partial class App : System.Windows.Application
    {
        private EventWaitHandle _event;
        private SplashWindow _splash;
        private EULAWindow _eula;
        private bool _initialized;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _event = new EventWaitHandle(false, EventResetMode.ManualReset, "Global\\8F24B2C3-13A4-490A-ACD5-0D68C7BAEA81", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("Forensic Analysis Tool is already running.", "Forensic Analysis Tool");
                Current.Shutdown();
            }
            else
            {
                ThreadHelper.Instance.RunOnThread(delegate { (_splash = new SplashWindow()).Show(); });

                var op = ThreadHelper.Instance.RunOnThreadDelayed(delegate 
                {
                    Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                    {
                        Source = new Uri("/ForensicAnalysisTool;component/AppResources.xaml", UriKind.RelativeOrAbsolute)
                    });
                    CompositionHelper.Instance.RegisterModules(new[]
                    {
                        typeof(APTAT.Common.DependencyLocator),
                        typeof(APTAT.DeploymentCore.DependencyLocator),
                        typeof(APTAT.Valkyrie.DependencyLocator.DependencyLocator),
                        typeof(APTAT.DAL.DependencyLocator.DependencyLocator),
                        typeof(APTAT.Core.DependencyLocator.DependencyLocator),
                        typeof(DependencyLocator)
                    });
                    CompositionHelper.Instance.Verify();
                });

                var timer = new DispatcherTimer(DispatcherPriority.Normal, Current.Dispatcher);
                timer.Interval = TimeSpan.FromMilliseconds(1500);
                timer.Tick += TimerTick;
                timer.Start();

                op.Completed += DispatcherOperationCompleted;
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
                if (_eula != null)
                {
                    (sender as DispatcherTimer).Stop();
                    _splash?.Dispatcher.Invoke(_splash.Close);
                    _eula.ShowDialog();
                    return;
                }

                var window = new MainWindow();
                window.Loaded += (s, a) =>
                {
                    _eula = new EULAWindow();
                    _eula.Owner = window;
                };
                window.Show();
                window.Activate();
            }
        }

        private void DispatcherOperationCompleted(object sender, EventArgs e)
        {
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

            path = Path.Combine(path, "FAT");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, "trace.log");
            Trace.Listeners.Add(new TextWriterTraceListener(path, "FATListener"));
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
