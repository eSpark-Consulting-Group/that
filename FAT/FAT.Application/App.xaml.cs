using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using APTAT.Common;
using FAT.Application.Helpers;
using Microsoft.Win32;
using System.Reflection;
using FAT.Application.Attributes;
using System.Linq;

namespace FAT.Application
{
    public partial class App : System.Windows.Application
    {
        private EventWaitHandle _event;
        private SplashWindow _splash;
        private EULAWindow _eula;
        private bool _initialized;
        private string _eulaVersion;
        private string _eulaCurrentVersion;

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

                ThreadHelper.Instance.RunOnThread(delegate
                {
                    Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                    {
                        Source = new Uri("/ForensicAnalysisTool;component/AppResources.xaml", UriKind.RelativeOrAbsolute)
                    });
                    CompositionHelper.Instance.RegisterModules(new[]
                    {
                        typeof(APTAT.Common.DependencyLocator),
                        typeof(APTAT.DeploymentCore.DependencyLocator),
                        typeof(APTAT.DAL.DependencyLocator.DependencyLocator),
                        typeof(APTAT.Core.DependencyLocator.DependencyLocator),
                        typeof(DependencyLocator)
                    });
                    CompositionHelper.Instance.Verify();
                    _initialized = true;
                });

                ThreadHelper.Instance.RunOnThread(delegate
                {
                    _eulaCurrentVersion = typeof(EULAVersionAttribute).Assembly.GetEULAVersionAttribute();

                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\COMODO\FAT"))
                    {
                        if (key != null)
                            _eulaVersion = key.GetValue("EULAVersion") as string;
                        else
                            _eulaVersion = string.Empty;
                    }
                });

                var timer = new DispatcherTimer(DispatcherPriority.Normal, Current.Dispatcher);
                timer.Interval = TimeSpan.FromMilliseconds(1500);
                timer.Tick += TimerTick;
                timer.Start();

                Current.DispatcherUnhandledException += AppDispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                InitTraceListener();
            }

            base.OnStartup(e);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (_initialized && _eulaVersion != null)
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
                    if (!string.Equals(_eulaCurrentVersion, _eulaVersion))
                    {
                        _eula = new EULAWindow();
                        _eula.Owner = window;
                    }
                    else
                    {
                        (sender as DispatcherTimer).Stop();
                        _splash?.Dispatcher.Invoke(_splash.Close);
                        Dispatcher.Yield();
                        window.CallOnLoaded();
                    }
                };
                window.Show();
                window.Activate();
            }
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
