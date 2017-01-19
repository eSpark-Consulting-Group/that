using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestAT.UIMaps.UIMapMainClasses;
using CodedUITestAT.UIMaps.UIMapScanCompletedWindowClasses;
using CodedUITestAT.UIMaps.StartScanWizard.UIMapStartScanWizardClasses;

namespace CodedUITestAT
{
    /// <summary> A common part for scan-related CodedUI classes. </summary>
    [CodedUITest]
    public abstract class CodedUITestCommon
    {
        public CodedUITestCommon()
        {
        }

        #region UIMap properties

        /// <summary> UIMap for Main screen, UIMapMain.uitest. </summary>
        public UIMapMain UIMapMain
        {
            get
            {
                if ((this.mapMain == null))
                {
                    this.mapMain = new UIMapMain();
                }

                return this.mapMain;
            }
        }
        private UIMapMain mapMain;

        /// <summary> UIMap for Scan Completed window, UIMapScanCompletedWindow.uitest. </summary>
        public UIMapScanCompletedWindow UIMapScanCompletedWindow
        {
            get
            {
                if ((this.mapScanCompletedWindow == null))
                {
                    this.mapScanCompletedWindow = new UIMapScanCompletedWindow();
                }

                return this.mapScanCompletedWindow;
            }
        }
        private UIMapScanCompletedWindow mapScanCompletedWindow;

        /// <summary> UIMap for Start Scanning Wizard screen, UIMapStartScanWizard.uitest. </summary>
        public UIMapStartScanWizard UIMapStartScanWizard
        {
            get
            {
                if ((this.mapStartScanWizard == null))
                {
                    this.mapStartScanWizard = new UIMapStartScanWizard();
                }

                return this.mapStartScanWizard;
            }
        }
        private UIMapStartScanWizard mapStartScanWizard;

        #endregion

        #region Control methods

        #region Scan Results window

        internal WpfText ScanCompletedTextBlock()
        {
            return UIMapScanCompletedWindow.UIUnknownFileHunterWindow.UIScanCompletedDoyouwaText;
        }

        #endregion

        #region Results panel

        internal WpfText LookupInProgressCounterTextBlock()
        {
            return UIMapMain.UIUnknownFileHunterWindow1.UIViewsMainViewxamlPane.UIItem0Text2;
        }

        internal WpfText UnknownFilesCounterTextBlock()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIItem0Text;
        }

        internal WpfText MaliciousFilesCounterTextBlock()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIItem0Text1;
        }

        #endregion

        #endregion

        #region Helper methods

        /// <summary> Waits for Scan Completed window. </summary>
        /// <param name="scanType">Set a scan type.</param>
        internal void WaitForScanCompleted(ScanType scanType)
        {
            int waitTime;

            switch (scanType)
            {
                case ScanType.Quick:
                    waitTime = Settings.WaitingTime.Default.QuickScanWaitTime;
                    break;
                case ScanType.Full:
                    waitTime = Settings.WaitingTime.Default.FullScanWaitTime;
                    break;
                default: throw new Exception(String.Format("Unexpected scan type '{0}'!", scanType));
            }

            UIHelper.WaitForControl(ScanCompletedTextBlock(), waitTime);
        }

        /// <summary> Waits for no 'Lookup in Progress' files count. </summary>
        internal void WaitForNoLookupInProgress()
        {
            int waitTime = Settings.WaitingTime.Default.LookupInProgressWaitTime;

            var timer = Stopwatch.StartNew();
            for (;;)
            {
                Playback.Wait(1000);
                if (LookupInProgressCounterTextBlock().DisplayText == "0") break;
                if ((int) timer.ElapsedMilliseconds > waitTime)
                            throw new Exception(String.Format("Results panel shows 'Lookup in Progress' {0} after {1} seconds!", LookupInProgressCounterTextBlock().DisplayText, waitTime));
            }
            timer.Stop();
        }

        #endregion
    }
}
