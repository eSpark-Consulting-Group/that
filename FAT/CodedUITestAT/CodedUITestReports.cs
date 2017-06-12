using System;
using System.Diagnostics;
using CodedUITestFAT.Helpers;
using CodedUITestFAT.UIMaps.UIMapReportsClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;


namespace CodedUITestFAT
{
    /// <summary> Tests for reports. </summary>
    [CodedUITest]
    public class CodedUITestReports : CodedUITestCommon
    {
        public CodedUITestReports()
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

            UIHelper.WaitForControlIsEnabled(SkipDiscoveryButton());
            UIMapDiscovery.ClickSkipDiscoveryButton();
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

        private WpfButton ShowInFolderButton()
        {
            return UIMapReports.UIUnknownFileHunterWindow.UIReportWindowCustom.UIRadToolBarToolBar1.UISystemWindowsControlButton;
        }

        private WpfText SendingFilesTextBlock()
        {
            return UIMapResultsWindow.UIUnknownFileHunterWindow1.UIViewsMainViewxamlPane.UISendingFilesforAnalyProgressBar.UISendingFilesforAnalyText;
        }

        private WpfText MainWindowTitleTextBlock()
        {
            return UIMapMain.UIUnknownFileHunterWindow1.UIUnknownFileHunterText;
        }

        #endregion

        #region Local helper methods

        /// <summary> Asserts whether an opened report pdf-view contains a given text. </summary>        
        /// <param name="textToFind">Set a text to find in the report pdf-view.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private void AssertReportPdfViewContains(string textToFind, TestContext testContext)
        {
            UIMapReports.AssertReportPdfViewerTextContainsExpectedValues.UIReportPdfViewerEditText = textToFind;
            UIMapReports.AssertReportPdfViewerTextContains();
            testContext.WriteLine(String.Format("Opened report pdf-view contains text: '{0}'.", textToFind));
        }

        /// <summary> Asserts a given report pdf-view title. </summary>
        /// <param name="report">Set a report.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private void AssertReportPdfViewTitle(Report report, TestContext testContext)
        {
            string title = report.Title.ToUpperFirst();

            UIMapReports.AssertReportTitleExpectedValues.UIExecutiveReportTextDisplayText = title;
            UIMapReports.AssertReportTitle();
            testContext.WriteLine(String.Format("Opened report pdf-view title '{0}' is correct.", title));
        }

        /// <summary> Waits for a report pdf-viewer screen. </summary>
        /// <param name="report">Set a report.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private void WaitForReportViewer(Report report, TestContext testContext)
        {
            int reportCreationTime = Settings.WaitingTime.Default.ReportCreationWaitTime;
            
            UIHelper.WaitForControl(ShowInFolderButton(), reportCreationTime);
            UIHelper.WaitForControlIsClickable(ShowInFolderButton());

            var timer = Stopwatch.StartNew();
            for (; ; )
            {
                Playback.Wait(1000);
                
                try
                {
                    AssertReportPdfViewTitle(report, testContext);
		          //AssertReportPdfViewContains(" ", testContext);
                  break;
	            }
	            catch (Exception) { }

                if ((int)timer.ElapsedMilliseconds > reportCreationTime)
                    throw new Exception(String.Format("Report pdf-viewer screen is not available after {0} seconds!", reportCreationTime / 1000));
            }
            timer.Stop();

            testContext.WriteLine("Report pdf-viewer screen appeared...");
        }

        /// <summary> Checks whether a given report file exists. </summary>
        /// <param name="report">Set a report.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private void CheckReportFile(Report report, TestContext testContext)
        {
            if (report.IsThereReportFile()) testContext.WriteLine(String.Format("Report file '{0}' created after '{1}' exists.", report.FileNamePattern, report.ExpectedCreationTime));
            else Assert.Fail(string.Format("Report file '{0}' created after '{1}' does not exist!", report.FileNamePattern, report.ExpectedCreationTime));        
        }
        
        /// <summary> Generates a report at Main screen and performs a basic report test. After all closes the report. </summary>
        /// <param name="reportType">Set a report type.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private void BasicReportTest(ReportType reportType, TestContext testContext)
        {
            Report report = new Report(reportType);
            report.Generate();
            report.WaitForReportFile();
            WaitForReportViewer(report, testContext);

            CheckReportFile(report, testContext);
            AssertReportPdfViewTitle(report, testContext);
            
            // The report viewer pdf text is no longer available for CodedUI Test after Telerik controls update
            /*
            if (report.Type == ReportType.ExecutiveValkyrie || report.Type == ReportType.PerDeviceValkyrie || report.Type == ReportType.PerProgramValkyrie)
                AssertReportPdfViewContains(Settings.Reports.Default.ValkyrieOnlyVerdict, testContext);
            */
            UIMapReports.ClickCloseReportButton();
        }

        /// <summary> Waits until the Valkyrie results are received. </summary>
        /// <param name="testContext">Set a current TestContext.</param>
        internal void WaitForValkyrieResults()
        {
            int waitTime = Settings.WaitingTime.Default.ValkyrieAnalysisWaitTime;
            UIHelper.WaitForControl(ValkyrieAnalysisResultsTab(), waitTime);
            UIHelper.WaitForControlIsEnabled(ValkyrieAnalysisResultsTab(), waitTime);
            TestContext.WriteLine("'Valkyrie analysis results' tab is enabled...");

            // Waits until the Valkyrie upload progress is completed
            UIHelper.WaitForControlIsClickable(SendingFilesTextBlock(), waitTime, false);
            var timer = Stopwatch.StartNew();
            for (;;)
            {
                Playback.Wait(2000);
                if (!MainWindowTitleTextBlock().DisplayText.Contains("%"))
                {
                    TestContext.WriteLine("Valkyrie upload progress is completed...");
                    break;
                }
                if ((int)timer.ElapsedMilliseconds > waitTime) throw new Exception(String.Format("Valkyrie upload progress is not completed after {0} minutes!", waitTime / 60000));
            }
            timer.Stop();

            // CodedUI for some reasons does not detect a tab content without switching/refreshing the tabs
            UIHelper.GroupByFiles();
            Playback.Wait(1000);
            UIHelper.GroupByComputers();

            // Waits until the results are actually available at UI
            int treeWaitTime = Settings.WaitingTime.Default.WpfTreeWaitTime;
            timer.Restart();
            for (;;)
            {
                Playback.Wait(2000);
                try
                {
                    ValkyrieAnalysisResultsTab().GetRows();
                    TestContext.WriteLine("Valkyrie results now can be obtained from UI.");
                    break;
                }
                catch (Exception) { }
                if ((int)timer.ElapsedMilliseconds > treeWaitTime) throw new Exception(String.Format("Valkyrie results cannot be obtained from UI after {0} seconds!", treeWaitTime / 1000));
            }
            timer.Stop();
        }

        #endregion

        #region Test methods

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void BuildBothExecutiveReportsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            SysHelper.InfectForScan(endpoint);
            SysHelper.AddUnknownFileForScan(endpoint);            
            CodedUITestNetworkAddresses useNetworkAddresses = new CodedUITestNetworkAddresses();

            UIMapDiscovery.ClickCustomScanButton();
            UIHelper.WaitForControl(NetworkAddressButton());
            UIMapStartScanWizard.ClickNetworkAddressButton();
            UIHelper.WaitForControlIsEnabled(NextButton());

            useNetworkAddresses.StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            //UIMapScanCompletedWindow.ClickNoButton();

            BasicReportTest(ReportType.Executive, TestContext);
            
            UIMapMain.ClickAnalyseWithValkyrieButton();
            //InputCamCredentials(TestContext);
            WaitForValkyrieResults();

            BasicReportTest(ReportType.ExecutiveValkyrie, TestContext);

            //UIHelper.WaitForControlIsClickable(useMain.StartScanButton());
        }

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void BuildBothPerDeviceReportsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            SysHelper.InfectForScan(endpoint);
            SysHelper.AddUnknownFileForScan(endpoint);            
            CodedUITestNetworkAddresses useNetworkAddresses = new CodedUITestNetworkAddresses();

            UIMapDiscovery.ClickCustomScanButton();
            UIHelper.WaitForControl(NetworkAddressButton());
            UIMapStartScanWizard.ClickNetworkAddressButton();
            UIHelper.WaitForControlIsEnabled(NextButton());

            useNetworkAddresses.StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            //useMain.UIMapScanCompletedWindow.ClickNoButton();

            BasicReportTest(ReportType.PerDevice, TestContext);

            UIMapMain.ClickAnalyseWithValkyrieButton();
            //useMain.InputCamCredentials(TestContext);
            WaitForValkyrieResults();
            
            BasicReportTest(ReportType.PerDeviceValkyrie, TestContext);
            
            //UIHelper.WaitForControlIsClickable(useMain.StartScanButton());
        }

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void BuildBothPerProgramReportsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            SysHelper.InfectForScan(endpoint);
            SysHelper.AddUnknownFileForScan(endpoint);            
            CodedUITestNetworkAddresses useNetworkAddresses = new CodedUITestNetworkAddresses();

            UIMapDiscovery.ClickCustomScanButton();
            UIHelper.WaitForControl(NetworkAddressButton());
            UIMapStartScanWizard.ClickNetworkAddressButton();
            UIHelper.WaitForControlIsEnabled(NextButton());

            useNetworkAddresses.StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            //useMain.UIMapScanCompletedWindow.ClickNoButton();

            BasicReportTest(ReportType.PerProgram, TestContext);

            UIMapMain.ClickAnalyseWithValkyrieButton();
            //useMain.InputCamCredentials(TestContext);
            WaitForValkyrieResults();

            BasicReportTest(ReportType.PerProgramValkyrie, TestContext);

            //UIHelper.WaitForControlIsClickable(useMain.StartScanButton());
        }
        #endregion
    }
}
