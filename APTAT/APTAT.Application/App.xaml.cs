using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using APTAT.Common;

namespace APTAT.Application
{
    public partial class App : System.Windows.Application
    {
        private Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _mutex = new Mutex(true, "Global\\8F24B2C3-13A4-490A-ACD5-0D68C7BAEA81", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("Unknown File Hunter is already running.", "Unknown File Hunter");

                _mutex = null;
                Current.Shutdown();
            }
            else
            {
                InitTraceListener();
                Current.DispatcherUnhandledException += AppDispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                CompositionHelper.Instance.RegisterModules(new[]
                {
                    typeof(Common.DependencyLocator),
                    typeof(DeploymentCore.DependencyLocator),
                    typeof(DependencyLocator)
                });
            }

            base.OnStartup(e);
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
