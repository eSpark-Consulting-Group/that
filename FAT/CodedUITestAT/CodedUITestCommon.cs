using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestFAT.UIMaps.UIMapMainClasses;
using CodedUITestFAT.UIMaps.UIMapDiscoveryClasses;
using CodedUITestAT.UIMaps.UIMapStopScanWindowClasses;
using CodedUITestFAT.UIMaps.UIMapResultsWindowClasses;
using CodedUITestFAT.UIMaps.UIMapAggregatedPanelClasses;
using CodedUITestFAT.UIMaps.Details.UIMapDetailsWindowClasses;
using CodedUITestAT.UIMaps.Details.UIMapValkyrieDetailsWindowClasses;
using CodedUITestFAT.UIMaps.StartScanWizard.UIMapStartScanWizardClasses;
using CodedUITestFAT.UIMaps.StartScanWizard.UIMapActiveDirectoryClasses;
using CodedUITestFAT.UIMaps.StartScanWizard.UIMapNetworkAddressesClasses;
using CodedUITestFAT.BusinessObjects.Reports;
using CodedUITestFAT.Helpers;

namespace CodedUITestFAT
{
    /// <summary> A common part for scan-related CodedUI classes. </summary>
    [CodedUITest]
    public abstract class CodedUITestCommon
    {
        //TODO remove to base test class
        #region 
        //internal CodedUITestFAT.Helpers.Assert Assert
        //{
        //    get
        //    {
        //        if ((this.assert == null))
        //        {
        //            this.assert = new CodedUITestFAT.Helpers.Assert();
        //        }
        //        return this.assert;
        //    }
        //}
        //private CodedUITestFAT.Helpers.Assert assert;
        #endregion

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

        /// <summary> UIMap for Network Addresses Scanning Wizard screen branch, UIMapNetworkAddresses.uitest. </summary>
        public UIMapNetworkAddresses UIMapNetworkAddresses
        {
            get
            {
                if ((this.mapNetworkAddresses == null))
                {
                    this.mapNetworkAddresses = new UIMapNetworkAddresses();
                }

                return this.mapNetworkAddresses;
            }
        }
        private UIMapNetworkAddresses mapNetworkAddresses;

        /// <summary> UIMap for Active Directory Scanning Wizard screen branch, UIMapActiveDirectory.uitest. </summary>
        public UIMapActiveDirectory UIMapActiveDirectory
        {
            get
            {
                if ((this.mapActiveDirectory == null))
                {
                    this.mapActiveDirectory = new UIMapActiveDirectory();
                }

                return this.mapActiveDirectory;
            }
        }
        private UIMapActiveDirectory mapActiveDirectory;

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

        /// <summary> UIMap for Valkyrie Details window, UIMapValkyrieDetailsWindow.uitest. </summary>
        public UIMapValkyrieDetailsWindow UIMapValkyrieDetailsWindow
        {
            get
            {
                if ((this.mapValkyrieDetailsWindow == null))
                {
                    this.mapValkyrieDetailsWindow = new UIMapValkyrieDetailsWindow();
                }

                return this.mapValkyrieDetailsWindow;
            }
        }
        private UIMapValkyrieDetailsWindow mapValkyrieDetailsWindow;

        /// <summary> UIMap for Details window, UIMapDetailsWindow.uitest. </summary>
        public UIMapDetailsWindow UIMapDetailsWindow
        {
            get
            {
                if ((this.mapDetailsWindow == null))
                {
                    this.mapDetailsWindow = new UIMapDetailsWindow();
                }

                return this.mapDetailsWindow;
            }
        }
        private UIMapDetailsWindow mapDetailsWindow;

        public UIMapAggregatedPanel UIMapAggregatedPanel
        {
            get
            {
                if ((this.mapAggregatedPanel == null))
                {
                    this.mapAggregatedPanel = new UIMapAggregatedPanel();
                }

                return this.mapAggregatedPanel;
            }
        }
        private UIMapAggregatedPanel mapAggregatedPanel;

        internal Log Log
        {
            get
            {
                if ((this.log == null))
                {
                    this.log = new Log(TestContext);
                }
                return this.log;
            }
        }
        private Log log;


        #endregion

        #region Control methods

        #region Results panel

        internal WpfText CleanFilesCounterTextBlock()
        {
            return UIMapAggregatedPanel.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIAggregatedPanelListBList.UICleanCounterListItemListItem.UIItem120100Text;
        }

        internal WpfText InAnalysisFilesCounterTextBlock()
        {
            return UIMapAggregatedPanel.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIAggregatedPanelListBList.UIInAnalysisCounterLisListItem.UIItem1021Text;
        }

        internal WpfText UnknownFilesCounterTextBlock()
        {
            return UIMapAggregatedPanel.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIAggregatedPanelListBList.UIUnknownCounterListItListItem.UIItem1009Text;
        }

        internal WpfText MaliciousFilesCounterTextBlock()
        {
            return UIMapAggregatedPanel.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIAggregatedPanelListBList.UIMalwareCounterListItListItem.UIItem3028Text;
        }

        #endregion

        #region Results screen

        internal WpfButton CloseSummaryWindowButton()
        {
            return UIMapResultsWindow.UIForensicAnalysisToolWindow.UICloseCrossButtonButton;
        }

        internal WpfToggleButton GroupByFilesToggleButton()
        {
            return UIMapResultsWindow.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIGroupByFilesToggleBuToggleButton;
        }

        internal WpfButton DetailedScanResultsButton()
        {
            return UIMapResultsWindow.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIPleaseWaitProgressBar.UIDetailedScanResultsBButton;
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

        #region Start Scan Wizard

        internal WpfButton NetworkAddressButton()
        {
            return UIMapStartScanWizard.UIForensicAnalysisToolWindow.UIScanningWizardWindowCustom.UIPleaseWaitProgressBar.UIScancomputerswithspeButton;
        }

        internal WpfButton QuickScanButton()
        {
            return UIMapStartScanWizard.UIForensicAnalysisToolWindow.UIScanningWizardWindowCustom.UIPleaseWaitProgressBar.UIQuickScanButton;
        }

        internal WpfButton NextButton()
        {
            return UIMapStartScanWizard.UIForensicAnalysisToolWindow.UIScanningWizardWindowCustom.UILoadingProgressBar.UINextButton;
        }

        #endregion
        internal WpfText ErrorTextBlock()
        {
            return UIMapStartScanWizard.UIForensicAnalysisToolWindow.UIScanningWizardWindowCustom.UIPleaseWaitProgressBar.UIPleasespecifyPassworText;
        }

        #endregion

        #region Helper methods

        internal WpfText GetResultTileTextControl(TileType type)
        {
            WpfText control = null;
            switch (type)
            {
                case TileType.Clean:
                    control = UIMapAggregatedPanel.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIAggregatedPanelListBList.UICleanCounterListItemListItem.UIItem120100Text;
                    break;
                case TileType.InAnalysis:
                    control = UIMapAggregatedPanel.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIAggregatedPanelListBList.UIInAnalysisCounterLisListItem.UIItem1021Text;
                    break;
                case TileType.Malware:
                    control = UIMapAggregatedPanel.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIAggregatedPanelListBList.UIMalwareCounterListItListItem.UIItem3028Text;
                    break;
                case TileType.NotAnalyzed:
                    control = UIMapAggregatedPanel.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIAggregatedPanelListBList.UINotAnalyzedCounterLiListItem.UIItem3017Text;
                    break;
                case TileType.Unknown:
                    control = UIMapAggregatedPanel.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIAggregatedPanelListBList.UIUnknownCounterListItListItem.UIItem1009Text;
                    break;
                default:
                    throw new ArgumentException("Unknown tile type");                    
            }
            return control;
        }

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
        internal void WaitForNoInAnalysis()
        {
            int waitTime = Settings.WaitingTime.Default.LookupInProgressWaitTime;

            var timer = Stopwatch.StartNew();
            do
            {
                if (InAnalysisFilesCounterTextBlock().DisplayText == "0 (0%)") return;
                //if (!InAnalysisFilesCounterTextBlock().IsVisible()) return;
                Playback.Wait(1000);
            } while ((int)timer.ElapsedMilliseconds < waitTime);
            throw new Exception(String.Format("Results panel shows 'Lookup in Progress' {0} after {1} seconds!", InAnalysisFilesCounterTextBlock().DisplayText, waitTime / 1000));
        }

        /// <summary> Refreshes the results screen using Search box. </summary>
        internal void RefreshResultsScreen()
        {
            UIMapMain.InputTextToSearchBoxParams.UISearchWatermarkTextBEditSendKeys = Guid.NewGuid().ToString();
            UIMapMain.InputTextToSearchBox();
            Playback.Wait(2000);
            UIMapMain.ClickClearSearchTextButton();
            Playback.Wait(2000);
        }

        /// <summary> Inputs a password. </summary>
        /// <param name="password">Set a password to input.</param>
        internal void InputPassword(string password)
        {
            WpfEdit passwordEdit = UIMapNetworkAddresses.UIForensicAnalysisToolWindow.UIThisCustom.UIPasswordWatermarkTexEdit;
            if (passwordEdit.Text == Settings.General.Default.EmptyWatermarkTextBoxText)
            {
                UIMapNetworkAddresses.InputPasswordParams.UIPasswordWatermarkTexEditSendKeys = password.Remove(1);
                UIMapNetworkAddresses.InputPasswordParams.UIPasswordBoxEditSendKeys = Playback.EncryptText(password.Substring(1));
                UIMapNetworkAddresses.InputPassword();
            }
        }

        /// <summary> Inputs a login, if necessary. </summary>
        /// <param name="login">Set a login to input.</param>
        internal void InputScanLogin(string login)
        {
            WpfEdit loginEdit = UIMapNetworkAddresses.UIForensicAnalysisToolWindow.UIThisCustom.UILoginWatermarkTextBoEdit;
            if (loginEdit.Text == Settings.General.Default.EmptyWatermarkTextBoxText || loginEdit.Text != login) loginEdit.Text = login;
        }

        /// <summary> Asserts a text of regular error message. </summary>
        /// <param name="expectedErrorText">Set expected error text.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        internal void AssertErrorText(string expectedErrorText, TestContext testContext)
        {
            UIHelper.WaitForControlIsClickable(ErrorTextBlock());
            UIMapStartScanWizard.AssertErrorTextBlockTextExpectedValues.UIPleasespecifyPassworTextDisplayText = expectedErrorText;
            UIMapStartScanWizard.AssertErrorTextBlockText();
            testContext.WriteLine(String.Format("Expected regular error message '{0}' appeared.", expectedErrorText));
        }
        #endregion

        #region Test Context property

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;      
        #endregion    
    }
}
