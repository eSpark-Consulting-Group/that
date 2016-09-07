
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using APTAT.Common;

namespace DDT.Application
{
    public partial class App : System.Windows.Application
    {
        private Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _mutex = new Mutex(true, "Global\\F49126C7-111C-46E5-B790-62B7BD1FD858", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("Auto Discovery and Deployment Tool is already running.", "Auto Discovery and Deployment Tool");

                _mutex = null;
                Current.Shutdown();
            }
            else
            {
                InitTraceListener();
                Current.DispatcherUnhandledException += AppDispatcher_UnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                CompositionHelper.Instance.RegisterModules(new[]
                {
                    typeof(APTAT.Common.DependencyLocator),
                    typeof(APTAT.DeploymentCore.DependencyLocator),
                    typeof(Core.DependencyLocator),
                    typeof(DependencyLocator)
                });
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.Dispose();

            base.OnExit(e);
        }

        private void AppDispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG 
            e.Handled = false;
#else
            e.Handled = true;
            LogHelper.Trace(TraceEventType.Error, e.Exception + (e.Exception.InnerException != null ? "\n InnerException :" + (e.Exception).InnerException : null));
#endif
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogHelper.Trace(TraceEventType.Error, e.ExceptionObject + (e.ExceptionObject is Exception && ((Exception)e.ExceptionObject).InnerException != null ? "\n InnerException : " + ((Exception)e.ExceptionObject).InnerException : null));
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
    }
}
