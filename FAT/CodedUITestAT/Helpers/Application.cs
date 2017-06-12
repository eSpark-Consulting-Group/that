using System;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace CodedUITestFAT.Helpers
{
    public static class Application
    {
        private static ApplicationUnderTest application;
        private static string applicationDirctory;
        private static string applicationExecutable;
        private static string processName;
        private static string applicationTitle;
        private static bool closeOnCleanup;
        public static void Init(string applicationDirctory, string applicationExecutable, string processName, string applicationTitle, bool closeOnCleanup)
        {
            Application.applicationDirctory = applicationDirctory;
            Application.applicationExecutable = applicationExecutable;
            Application.processName = processName;
            Application.applicationTitle = applicationTitle;
            Application.closeOnCleanup = closeOnCleanup;
        }


        public static ApplicationUnderTest Start(this ApplicationUnderTest app)
        {
            Process appProc = null;
            ApplicationUnderTest application = null;

            string applicationPath = Path.Combine(applicationDirctory, applicationExecutable);
            var procStartInfo = new ProcessStartInfo(applicationPath);
            procStartInfo.WorkingDirectory = applicationDirctory;
            Process proc = Process.Start(procStartInfo);
            if (proc == null) throw new Exception(string.Format("Process '{0}' was not started!", applicationPath));
            Playback.Wait(5000);

            foreach (var item in Process.GetProcessesByName(processName))
            {
                if (item.MainWindowTitle.Equals(applicationTitle))
                {
                    appProc = item;
                }
            }

            if (appProc != null)
            {
                application = ApplicationUnderTest.FromProcess(appProc);
                application.CloseOnPlaybackCleanup = closeOnCleanup;
            }
            return application;
        }
    }

    public static class AUTExtensions
    {

    }
}

