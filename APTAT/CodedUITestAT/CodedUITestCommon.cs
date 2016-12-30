using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
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

        #region Scan Completed window

        private WpfText ScanCompletedTextBlock()
        {
            return UIMapScanCompletedWindow.UIUnknownFileHunterWindow.UIScanCompletedDoyouwaText;
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

        #endregion
    }
}
