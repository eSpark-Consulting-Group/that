
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using APTAT.Common;

namespace DDT.Application
{
    public partial class App : System.Windows.Application, IDisposable
    {
        private Mutex _mutex;
        private static string appGuid = "F49126C7-111C-46E5-B790-62B7BD1FD858";
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Current.DispatcherUnhandledException += AppDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            CompositionHelper.Instance.RegisterModules(new[] 
            {
                typeof(APTAT.Common.DependencyLocator),
                typeof(APTAT.DeploymentCore.DependencyLocator),
                typeof(Core.DependencyLocator),
                typeof(DependencyLocator)
            });

            InitTraceListener();
            LogHelper.Trace(TraceEventType.Information, TraceCategory.Descent,$"DDT started: {DateTime.UtcNow}");
        }

        public void Dispose()
        {
            if (_mutex != null)
            {
                _mutex.Dispose();
                _mutex = null;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _mutex = new Mutex(true, "Global\\" + appGuid, out createdNew);
            if (!createdNew)
            {
                _mutex = null;
                MessageBox.Show("Auto Discovery and Deployment Tool is already running","Auto Discovery and Deployment Tool");
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG // In debug mode do not custom-handle the exception, let Visual Studio handle it
                e.Handled = false;

#else

            ShowUnhandeledException(e);
#endif
        }
        private void ShowUnhandeledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            LogHelper.Trace(TraceEventType.Error, e.Exception + (e.Exception.InnerException != null ? "\n InnerException :" + (e.Exception).InnerException : null));
        }
        private static void InitTraceListener()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Comodo");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, "DDT");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, "ddt.log");
            Trace.Listeners.Add(new TextWriterTraceListener(path, "DDTListener"));
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogHelper.Trace(TraceEventType.Error, e.ExceptionObject + (e.ExceptionObject is Exception && ((Exception)e.ExceptionObject).InnerException != null ? "\n InnerException : " + ((Exception)e.ExceptionObject).InnerException : null));
        }
    }
}
