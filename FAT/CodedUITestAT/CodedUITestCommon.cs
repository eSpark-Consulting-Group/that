using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestAT.UIMaps.UIMapMainClasses;
using CodedUITestAT.UIMaps.StartScanWizard.UIMapStartScanWizardClasses;
using CodedUITestAT.UIMaps.UIMapStopScanWindowClasses;
using CodedUITestAT.UIMaps.UIMapDiscoveryClasses;
using CodedUITestAT.UIMaps.UIMapResultsWindowClasses;

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

        /// <summary> UIMap for Stop Scan window, UIMapStopScanWindow.uitest. </summary>
        public UIMapStopScanWindow UIMapStopScanWindow
        {
            get
            {
                if ((this.mapStopScanWindow == null))
                {
                    this.mapStopScanWindow = new UIMapStopScanWindow();
                }

                return this.mapStopScanWindow;
            }
        }
        private UIMapStopScanWindow mapStopScanWindow;

        /// <summary> UIMap for Discovery window, UIMapDiscovery.uitest. </summary>
        public UIMapDiscovery UIMapDiscovery
        {
            get
            {
                if ((this.mapDiscovery == null))
                {
                    this.mapDiscovery = new UIMapDiscovery();
                }

                return this.mapDiscovery;
            }
        }
        private UIMapDiscovery mapDiscovery;

        /// <summary> UIMap for Results window, UIMapResultsWindow.uitest. </summary>
        public UIMapResultsWindow UIMapResultsWindow
        {
            get
            {
                if ((this.mapResultsWindow == null))
                {
                    this.mapResultsWindow = new UIMapResultsWindow();
                }

                return this.mapResultsWindow;
            }
        }
        private UIMapResultsWindow mapResultsWindow;

        #endregion

        #region Control methods

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

        #region Results screen

        internal WpfButton CloseSummaryWindowButton()
        {
            return UIMapResultsWindow.UIForensicAnalysisToolWindow.UICloseCrossButtonButton;
        }

        #endregion

        #region Stop Scan window

        internal WpfButton YesDialogButton()
        {
            return UIMapStopScanWindow.UIForensicAnalysisToolWindow.UIYesButton;
        }

        #endregion

        #region Discovery screen

        internal WpfButton SkipDiscoveryButton()
        {
            return UIMapDiscovery.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UISkipDiscoveryButtonButton;
        }

        #endregion

        #region Main screen

        internal WpfButton ReportsButton()
        {
            return UIMapMain.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIMainToolBarToolBar.UIReportsButton;
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
                        
            UIHelper.WaitForControl(CloseSummaryWindowButton());
            UIHelper.WaitForControlIsClickable(CloseSummaryWindowButton(), waitTime);
            UIMapResultsWindow.ClickCloseSummaryWindowButton();
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
                            throw new Exception(String.Format("Results panel shows 'Lookup in Progress' {0} after {1} seconds!", LookupInProgressCounterTextBlock().DisplayText, waitTime / 1000));
            }
            timer.Stop();
        }

        #endregion
    }
}
