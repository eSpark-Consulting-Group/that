using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestAT.UIMaps.UIMapReportsClasses;

namespace CodedUITestAT
{
    /// <summary> Tests for reports. </summary>
    [CodedUITest]
    public class CodedUITestReports
    {
        public CodedUITestReports()
        {
        }

        #region UIMap properties

        public UIMapReports UIMapReports
        {
            get
            {
                if ((this.mapReports == null))
                {
                    this.mapReports = new UIMapReports();
                }

                return this.mapReports;
            }
        }
        private UIMapReports mapReports;

        #endregion

        #region Initialize and cleanup

        [TestInitialize]
        public void ATTestInitialize()
        {
            SysHelper.StartApplication(DataHelper.GetEndpoint());

            CodedUITestEula useEula = new CodedUITestEula();
            CodedUITestMain useMain = new CodedUITestMain();
            useEula.UIMapEula.ClickIAcceptButton();

            UIHelper.WaitForControlIsEnabled(useMain.StartScanningButton());
        }

        [TestCleanup]
        public void ATTestCleanup()
        {
            Computer endpoint = DataHelper.GetEndpoint();

            SysHelper.StopApplication();
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            SysHelper.StopScanner(endpoint);
        }

        #endregion

        #region Control methods

        private WpfButton ShowInFolderButton()
        {
            return UIMapReports.UIUnknownFileHunterWindow.UIReportWindowCustom.UIRadToolBarToolBar1.UISystemWindowsControlButton;
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
            else Assert.Fail(String.Format("Report file '{0}' created after '{1}' does not exist!", report.FileNamePattern, report.ExpectedCreationTime));        
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
               AssertReportPdfViewContains(report.Title, testContext);

            if (report.Type == ReportType.ExecutiveValkyrie || report.Type == ReportType.PerDeviceValkyrie || report.Type == ReportType.PerProgramValkyrie)
                AssertReportPdfViewContains(Settings.Reports.Default.ValkyrieOnlyVerdict, testContext);
            */
            UIMapReports.ClickCloseReportButton();
        }

        #endregion

        #region Test methods

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void BuildBothExecutiveReportsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            SysHelper.InfectForScan(endpoint);
            SysHelper.AddUnknownFileForScan(endpoint);
            CodedUITestMain useMain = new CodedUITestMain();
            
            useMain.StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            useMain.WaitForScanCompleted(ScanType.Quick);
            useMain.UIMapScanCompletedWindow.ClickCancelButton();

            BasicReportTest(ReportType.Executive, TestContext);
            
            useMain.UIMapMain.ClickAnalyseWithValkyrieButton();
            useMain.InputCamCredentials(TestContext);
            useMain.WaitForValkyrieResults(TestContext);

            BasicReportTest(ReportType.ExecutiveValkyrie, TestContext);

            UIHelper.WaitForControlIsClickable(useMain.StartScanningButton());
        }

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void BuildBothPerDeviceReportsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            SysHelper.InfectForScan(endpoint);
            SysHelper.AddUnknownFileForScan(endpoint);
            CodedUITestMain useMain = new CodedUITestMain();

            useMain.StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            useMain.WaitForScanCompleted(ScanType.Quick);
            useMain.UIMapScanCompletedWindow.ClickCancelButton();

            BasicReportTest(ReportType.PerDevice, TestContext);

            useMain.UIMapMain.ClickAnalyseWithValkyrieButton();
            useMain.InputCamCredentials(TestContext);
            useMain.WaitForValkyrieResults(TestContext);
            
            BasicReportTest(ReportType.PerDeviceValkyrie, TestContext);
            
            UIHelper.WaitForControlIsClickable(useMain.StartScanningButton());
        }

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void BuildBothPerProgramReportsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            SysHelper.InfectForScan(endpoint);
            SysHelper.AddUnknownFileForScan(endpoint);
            CodedUITestMain useMain = new CodedUITestMain();

            useMain.StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            useMain.WaitForScanCompleted(ScanType.Quick);
            useMain.UIMapScanCompletedWindow.ClickCancelButton();

            BasicReportTest(ReportType.PerProgram, TestContext);

            useMain.UIMapMain.ClickAnalyseWithValkyrieButton();
            useMain.InputCamCredentials(TestContext);
            useMain.WaitForValkyrieResults(TestContext);

            BasicReportTest(ReportType.PerProgramValkyrie, TestContext);

            UIHelper.WaitForControlIsClickable(useMain.StartScanningButton());
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
