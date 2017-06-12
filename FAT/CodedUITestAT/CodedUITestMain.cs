using System;
using CodedUITestFAT.Helpers;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace CodedUITestFAT
{
    /// <summary> Tests for the Main screen. </summary>
    [CodedUITest]
    public sealed class CodedUITestMain : CodedUITestCommon
    {
        public CodedUITestMain()
        {
        }       

        #region Initialize and cleanup

        [TestInitialize]
        public void ATTestInitialize()
        {
            app = app.Start();
            WaitForAppIsRunning(app);

            if (!UIHelper.isEulaWasAccepted())
            {
                UIMapEula.ClickIAcceptButton();
            }            
        }
        
        [TestCleanup]
        public void ATTestCleanup()
        {
            Computer endpoint = DataHelper.GetEndpoint();

            SysHelper.StopProduct();
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            SysHelper.StopScanner(endpoint);
        }

        #endregion

        #region Control methods        

        #region Main screen      
        
        internal WpfButton PreviousScansButton()
        {
            return UIMapMain.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIMainToolBarToolBar.UIPreviousScansButtonButton;
        }       
        
        private WpfButton CreateReportDropDownButton()
        {
            return UIMapMain.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIMainToolBarToolBar.UICreateReportButton;
        }

        private WpfButton OptionsButton()
        {
            return UIMapMain.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIMainToolBarToolBar.UICreateReportButton;
        }          

        private WpfText ComodoOneReferalTextPart1()
        {
            return UIMapMain.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIPleaseWaitProgressBar.UIComodoForensicAnalysText1;
        }
        private WpfText ComodoOneReferalTextPart2()
        {
            return UIMapMain.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIPleaseWaitProgressBar.UIOnecomodocomText;
        }
        private WpfText ComodoOneReferalTextPart3()
        {
            return UIMapMain.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIPleaseWaitProgressBar.UITolearnhowComodocanhText;
        }

        #endregion      

        #region Details window

        private WpfButton CloseDetailsButton()
        {
            return UIMapDetailsWindow.UIForensicAnalysisToolWindow.UIThreatDetailsWindowCustom.UICloseButton;
        }

        #endregion

        #region Valkyrie Details window

        private WpfButton CloseValkyrieDetailsButton()
        {
            return UIMapValkyrieDetailsWindow.UIUnknownFileHunterWindow.UIValkyrieDetailsWindoCustom.UICloseButton;
        }

        #endregion

        #endregion

        #region Local helper methods

        /// <summary> Gets the number of files of a given type from results panel. </summary>
        /// <param name="fileType">Set a file type.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private int GetFoundFilesNumber(Verdict fileType, TestContext testContext)
        {
            string filesNumberStr;
            int filesNumber;

            switch (fileType)
            {
                case Verdict.Unknown:
                    filesNumberStr = UnknownFilesCounterTextBlock().DisplayText;
                    break;
                case Verdict.Malware:
                    filesNumberStr = MaliciousFilesCounterTextBlock().DisplayText;
                    break;
                default: throw new Exception(String.Format("Unexpected file type '{0}'!", fileType));
            }

            string searchWord = "(";
            string searchPattern = @"\d{1,}\s{1,}" + searchWord;
            string match = Regex.Match(filesNumberStr, searchPattern).ToString().Replace(searchWord, String.Empty);

            if (Int32.TryParse(filesNumberStr, out filesNumber))
                testContext.WriteLine(String.Format("'{0}' files number {1} successfully found at results panel.", fileType, filesNumber));
            else Assert.Fail(String.Format("'{0}' files number is not found at scan results panel!", fileType));

            return filesNumber;
        }

        /*
        /// <summary> Gets the number of files of a given type from Scan Results window. </summary>
        /// <param name="fileType">Set a file type.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private int GetFoundFilesNumber(Verdict fileType, TestContext testContext)
        {
            string searchWord = null;

            switch (fileType)
            {
                case Verdict.Unknown:
                    searchWord = Settings.ScanCompleted.Default.UnknownFile;
                    break;
                case Verdict.Malware:
                    searchWord = Settings.ScanCompleted.Default.Malware;
                    break;
                default: throw new Exception(String.Format("Unexpected file type '{0}'!", fileType));
            }
            
            string searchPattern = @"\d{1,}\s{1,}" + searchWord;
            string match = Regex.Match(ScanStatisticsTextBlock().DisplayText, searchPattern).ToString().Replace(searchWord, String.Empty);
            int filesNumber;
            if (Int32.TryParse(match, out filesNumber))
                testContext.WriteLine(String.Format("'{0}' files number {1} successfully found at Scan Completed window.", searchWord, filesNumber));
            else Assert.Fail(String.Format("'{0}' files number is not found at Scan Completed window!", searchWord));

            return filesNumber;
        }
        */
        /// <summary> Gets the total number of threats found from results panel. </summary>
        /// <param name="testContext">Set a current TestContext.</param>
        private int GetFoundFilesNumber(TestContext testContext)
        {
            int unknownFilesNumber = GetFoundFilesNumber(Verdict.Unknown, testContext);
            int malwaresNumber = GetFoundFilesNumber(Verdict.Malware, testContext);
            /*
            if (ScanStatisticsTextBlock().DisplayText.Contains(Settings.ScanCompleted.Default.Malware))
                malwaresNumber = GetFoundFilesNumber(Verdict.Malware, testContext);
            */
            return unknownFilesNumber + malwaresNumber;
        }

       

        /// <summary> Asserts the number of analyzed files at 'Valkyrie analysis results' screen. Starts at the mentioned screen. </summary>
        /// <param name="expectedFilesNumber">Set an expected files number.</param>
        /// <param name="testContext">Set a current test context.</param>
        private void AssertValkyrieAnalysisResults(int expectedFilesNumber, TestContext testContext)
        {
            int valkyrieResultsRowsNumber = ValkyrieAnalysisResultsTab().GetRows().Length;
            
            if (valkyrieResultsRowsNumber == expectedFilesNumber)
                testContext.WriteLine(String.Format("The total number of files found {0} and the number of files at 'Valkyrie analysis results' screen {1} coincide.", expectedFilesNumber, valkyrieResultsRowsNumber));
            else Assert.Fail(String.Format("The total number of files found {0} and the number of files at 'Valkyrie analysis results' screen {1} do not coincide!", expectedFilesNumber, valkyrieResultsRowsNumber));
        }

        /// <summary> Asserts the referal text are equal to expected string </summary>
        /// <param name="expectedReferralText">Set an expected referral text.</param>
        /// <param name="testContext">Set a current test context.</param>
        private void AssertReferralTextAreEqual(string expectedReferralText, TestContext testContext)
        {
            string actualReferaraltext = ComodoOneReferalTextPart1().DisplayText + ComodoOneReferalTextPart2().DisplayText + ComodoOneReferalTextPart3().DisplayText;            
            if (actualReferaraltext.Equals(expectedReferralText))
                testContext.WriteLine(String.Format("Actual referral text: \"{0}\" are equal to expected: \"{1}\"", actualReferaraltext, expectedReferralText));
            else Assert.Fail(String.Format("Actual referral text: \"{0}\" are not equal to expected: \"{1}\"", actualReferaraltext, expectedReferralText));
        }

       
       
        /// <summary> Sets a given scan status to the results screen filter. </summary>
        /// <param name="scanStatus">Set scan status.</param>
        private void SetStatusFilter(ScanStatus scanStatus, TestContext testContext)
        {
            UIMapResultsWindow.UIUnknownFileHunterWindow.UIScanResultsScreenByCCustom.UIFilteringDropDownCustom.UIFilterDropDownButtonButton.Container = ScanResultsTab();
            UIMapResultsWindow.ClickFilterDropDownButton();

            switch (scanStatus)
            {
                case ScanStatus.Completed:
                    UIMapResultsWindow.CheckCompletedFilterCheckBox();
                    break;
                case ScanStatus.Failed:
                    UIMapResultsWindow.CheckFailedFilterCheckBox();
                    break;
                case ScanStatus.Canceled:
                    UIMapResultsWindow.CheckCanceledFilterCheckBox();
                    break;
                case ScanStatus.InProgress:
                    UIMapResultsWindow.CheckInProgressFilterCheckBox();
                    break;
                case ScanStatus.NotAccessible:
                    UIMapResultsWindow.CheckOfflineFilterCheckBox();
                    break;
                default: throw new Exception(String.Format("Unexpected scan status!", scanStatus));
            }

            testContext.WriteLine(String.Format("Scan status '{0}' checked at the results screen filter.", scanStatus));
        }      
       
       
       

        #endregion

        #region Test methods 
       
        [TestMethod]
        [TestCategory("Smoke"), TestCategory("UiDefaults")]
        public void VerifyMainScreenDefaultsTest()
        {

            UIMapMain.ClickResizeButton();
            if (MainWindow().Maximized) TestContext.WriteLine("Main window was successfully maximized using Resize button.");
            else Assert.Fail("Main window was not maximized after clicking Resize button!");

            UIMapMain.ClickResizeButton();
            if (!MainWindow().Maximized) TestContext.WriteLine("Main window is not maximized after clicking Resize button again.");
            else Assert.Fail("Main window is maximized after clicking Resize button again!");

            UIMapMain.ClickMinimizeButton();
            if (MainWindow().Minimized) TestContext.WriteLine("Main window is successfully minimized after clicking Minimize button.");
            else Assert.Fail("Main window is not minimized after clicking Minimize button!");
            MainWindow().Minimized = false;

            if (SkipDiscoveryButton().Enabled) TestContext.WriteLine("Skip Discovery button is enabled by default.");
            else Assert.Fail("Skip Discovery button is enabled by default!");

            if (!PreviousScansButton().Enabled) TestContext.WriteLine("Previous Scans button is disabled by default.");
            else Assert.Fail("Previous Scans button is enabled by default!");           

            if (!CreateReportDropDownButton().Enabled) TestContext.WriteLine("Create Report drop down button is disabled by default.");
            else Assert.Fail("Stop Scan button is enabled by default!");

            if (!OptionsButton().Enabled) TestContext.WriteLine("Options button is disabled by default.");
            else Assert.Fail("Options button is enabled by default!");

            if (HelpButton().Enabled) TestContext.WriteLine("Help button is enabled by default.");
            else Assert.Fail("Help button is disabled by default!");

            UIHelper.WaitForControlIsEnabled(SkipDiscoveryButton());
            UIMapDiscovery.ClickSkipDiscoveryButton();            

            UIHelper.WaitForControlIsEnabled(CustomScanButton());
            UIHelper.WaitForControlIsClickable(CustomScanButton());

            if (ScanNowButton().Enabled) TestContext.WriteLine("'Scan Now' button is enabled by default.");
            else Assert.Fail("'Scan Now' button is disabled by default!");

            if (CustomScanButton().Enabled) TestContext.WriteLine("'Custom Scan' button is enabled by default.");
            else Assert.Fail("'Custom Scan' button is disabled by default!");

            if (StartAutoDiscoveryButton().Enabled) TestContext.WriteLine("'Start Auto Discovery' button is enabled by default.");
            else Assert.Fail("'Start Auto Discovery' button is disabled by default!");

            AssertReferralTextAreEqual(Settings.General.Default.ComodoOneReferralText, TestContext);

            UIMapMain.ClickComodoOneReferralLink();
            UIHelper.AssertBrowserUri(Settings.General.Default.ComodoOneReferralUri, TestContext);

            UIMapMain.ClickCloseButton();
            if (!MainWindow().Exists) TestContext.WriteLine("Main window was successfully closed using Close button.");
            else Assert.Fail("Main window was not closed after clicking Close button!");
        }
      
        #endregion      
    }
}