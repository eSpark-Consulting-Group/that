using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using APTAT.Application.Helpers;
using APTAT.Application.Services;
using APTAT.Core;


namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    /// 
    public partial class App : IDisposable
    {
        private Mutex _mutex;
        private static string appGuid = "8F24B2C3-13A4-490A-ACD5-0D68C7BAEA81";
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
            // thread should own mutex, so pass true
            _mutex = new Mutex(true, "Global\\" + appGuid, out createdNew);
            if (!createdNew)
            {
                _mutex = null;
                MessageBox.Show("Instance already running");
                Current.Shutdown(); // close application!
                return;
            }

            base.OnStartup(e);
            //run application code
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_mutex != null)
                _mutex.ReleaseMutex();
            base.OnExit(e);
        }


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Global exception handling  
            System.Windows.Application.Current.DispatcherUnhandledException += AppDispatcherUnhandledException;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            InitTraceListener();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            Trace.WriteLine(String.Format("APTAT Tool {0} started : {1}", version, DateTime.UtcNow));
            Trace.Flush();

            CompositionHelper.Instance.Register<ITargetsDiscoveryService, TargetsDiscoveryService>();
            CompositionHelper.Instance.Register<IAptService, AptService>();
            CompositionHelper.Instance.Register<IStorageService, StorageService>();
            CompositionHelper.Instance.Register<IWmiService, WmiService>();

            CompositionHelper.Instance.Verify();

            //Application.Properties.Settings.Default.ProxyLogin = "";
            //Application.Properties.Settings.Default.ProxyPassword = "";
            //Application.Properties.Settings.Default.Save();
        }

        private static void InitTraceListener()
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

            path = Path.Combine(path, "apttool.log");
            Trace.Listeners.Add(new TextWriterTraceListener(path, "ValkyrieListener"));
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogHelper.Trace(TraceEventType.Error, e.ExceptionObject + (e.ExceptionObject is Exception && ((Exception)e.ExceptionObject).InnerException != null ? "\n InnerException : " + ((Exception)e.ExceptionObject).InnerException : null));
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
            //string errorMessage = string.Format(
            //    "An application error occurred.\nPlease check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it.\n\nError:{0}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you click No the application will close)",
            //    e.Exception.Message + (e.Exception.InnerException != null ? "\n" + e.Exception.InnerException.Message : null));

            //if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
            //{
            //    if (MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            //    {
            //        System.Windows.Application.Current.Shutdown();
            //    }
            //}
        }
    }
}
