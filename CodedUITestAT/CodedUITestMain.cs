using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestAT.UIMaps.UIMapMainClasses;
using CodedUITestAT.UIMaps.StartScanningWizard.UIMapStartScanningWizardClasses;
using CodedUITestAT.UIMaps.StartScanningWizard.UIMapNetworkAddressesClasses;
using CodedUITestAT.UIMaps.StartScanningWizard.UIMapWorkgroupClasses;
using CodedUITestAT.UIMaps.UIMapScanCompletedWindowClasses;
using CodedUITestAT.UIMaps.UIMapCamCredentialsWindowClasses;
using CodedUITestAT.UIMaps.UIMapResultsWindowClasses;
using CodedUITestAT.UIMaps.UIMapDetailsWindowClasses;
using CodedUITestAT.UIMaps.UIMapStopScanWindowClasses;

namespace CodedUITestAT
{
    /// <summary> Tests for the Main screen. </summary>
    [CodedUITest]
    public sealed class CodedUITestMain
    {
        public CodedUITestMain()
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
        
        /// <summary> UIMap for Start Scanning Wizard screen, UIMapStartScanningWizard.uitest. </summary>
        public UIMapStartScanningWizard UIMapStartScanningWizard
        {
            get
            {
                if ((this.mapStartScanningWizard == null))
                {
                    this.mapStartScanningWizard = new UIMapStartScanningWizard();
                }

                return this.mapStartScanningWizard;
            }
        }
        private UIMapStartScanningWizard mapStartScanningWizard;

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

        /// <summary> UIMap for Workgroup Scanning Wizard screen branch, UIMapWorkgroup.uitest. </summary>
        public UIMapWorkgroup UIMapWorkgroup
        {
            get
            {
                if ((this.mapWorkgroup == null))
                {
                    this.mapWorkgroup = new UIMapWorkgroup();
                }

                return this.mapWorkgroup;
            }
        }
        private UIMapWorkgroup mapWorkgroup;

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

        /// <summary> UIMap for CAM Credentials window, UIMapCamCredentialsWindow.uitest. </summary>
        public UIMapCamCredentialsWindow UIMapCamCredentialsWindow
        {
            get
            {
                if ((this.mapCamCredentialsWindow == null))
                {
                    this.mapCamCredentialsWindow = new UIMapCamCredentialsWindow();
                }

                return this.mapCamCredentialsWindow;
            }
        }
        private UIMapCamCredentialsWindow mapCamCredentialsWindow;

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

        #endregion
        
        #region Initialize and cleanup

        [TestInitialize]
        public void ATTestInitialize()
        {
            SysHelper.StartApplication(DataHelper.GetEndpoint());
            
            CodedUITestEula useEula = new CodedUITestEula();
            useEula.UIMapEula.CheckIAcceptTermsOfThisAgreementCheckBox();
            useEula.UIMapEula.ClickContinueButton();

            UIHelper.WaitForControlIsEnabled(StartScanningButton());
        }
        
        [TestCleanup]
        public void ATTestCleanup()
        {
            SysHelper.StopApplication();
        }

        #endregion
        
        #region Control methods
        
        #region Main screen

        internal WpfWindow MainWindow()
        {
            return UIMapMain.UIUnknownFileHunterWindow;
        }
        
        internal WpfButton StartScanningButton()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIRadToolBarToolBar.UIStartScanningButtonButton;
        }

        private WpfButton StartScanningNowButton()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UISendingFilesforAnalyProgressBar.UIStartScanningNowButton;
        }

        private WpfButton StopScanningButton()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIRadToolBarToolBar.UIStopScanningButtonButton;
        }

        private WpfButton AnalyseWithValkyrieButton()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIRadToolBarToolBar.UIAnalyseWithValkyrieBButton;
        }

        private WpfButton ReportsDropDownButton()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIRadToolBarToolBar.UIReportButtonButton;
        }

        private WpfText MainWindowTitleTextBlock()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIUnknownFileHunterText;
        }

        private WpfHyperlink ValkyrieLink()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIPleaseclickheretoseeText.UIPleaseclickheretoseeHyperlink;
        }

        #endregion

        #region Start Scanning wizard

        private WpfButton NetworkAddressButton()
        {
            return UIMapStartScanningWizard.UIUnknownFileHunterWindow.UIScanningWizardWindowCustom.UIScancomputersbyspeciButton;
        }

        private WpfButton QuickScanButton()
        {
            return UIMapStartScanningWizard.UIUnknownFileHunterWindow.UIScanningWizardWindowCustom.UIQuickScanButton;
        }

        private WpfList NetworkAddressesList()
        {
            return UIMapNetworkAddresses.UIUnknownFileHunterWindow.UINetworkAddressesViewCustom.UITextEntryListViewCustom.UINetworkAddressItemsLList;
        }

        #endregion

        #region Scan Completed window

        private WpfText ScanCompletedTextBlock()
        {
            return UIMapScanCompletedWindow.UIUnknownFileHunterWindow.UIScanCompletedDoyouwaText;
        }

        private WpfText ScanStatisticsTextBlock()
        {
            return UIMapScanCompletedWindow.UIUnknownFileHunterWindow.UIComputerStatistics1TText;
        }

        #endregion

        #region CAM Credentials window

        private WpfEdit CamLoginTextBox()
        {
            return UIMapCamCredentialsWindow.UIUnknownFileHunterWindow.UILoginWatermarkTextBoEdit;
        }

        #endregion

        #region Results window

        internal WpfTabPage ScanningResultsTab()
        {     
            return UIMapResultsWindow.UIUnknownFileHunterWindow.UIResultsScreenCustom.UIResultsTabControlTabList.UIUnknownfilesTabPage;
        }

        internal WpfTabPage ValkyrieAnalysisResultsTab()
        {
            return UIMapResultsWindow.UIUnknownFileHunterWindow.UIResultsScreenCustom.UIResultsTabControlTabList.UIValkyrieanalysisresuTabPage;
        }

        internal WpfTree ScanningResultsTree()
        {
            return UIMapResultsWindow.UIUnknownFileHunterWindow.UIScanResultsScreenCustom.UIRadTreeListViewTree2;
        }

        #endregion

        #region Details window

        private WpfButton CloseDetailsButton()
        {
            return UIMapDetailsWindow.UIUnknownFileHunterWindow.UIThreatDetailsWindowCustom.UICloseButton;
        }

        #endregion

        #region Stop Scan window

        private WpfButton YesDialogButton()
        {
            return UIMapStopScanWindow.UIUnknownFileHunter0scWindow.UIYesButton;
        }

        #endregion

        #endregion

        #region Local helper methods

        /// <summary> Gets the number of files of a given type from Scan Completed window. Starts at Scan Completed window. </summary>
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
                testContext.WriteLine(String.Format("'{0}' files number {1} successfully found at Scan Completed window.\n", searchWord, filesNumber));
            else Assert.Fail(String.Format("'{0}' files number is not found at Scan Completed window!", searchWord));

            return filesNumber;
        }

        /// <summary> Gets the total number of files found from Scan Completed window. Starts at Scan Completed window. </summary>
        /// <param name="testContext">Set a current TestContext.</param>
        private int GetFoundFilesNumber(TestContext testContext)
        {
            int unknownFilesNumber = GetFoundFilesNumber(Verdict.Unknown, testContext);
            int malwaresNumber = 0;
            if (ScanStatisticsTextBlock().DisplayText.Contains(Settings.ScanCompleted.Default.Malware))
                malwaresNumber = GetFoundFilesNumber(Verdict.Malware, testContext);
            return unknownFilesNumber + malwaresNumber;
        }

        /// <summary> Asserts the number of analyzed files at 'Valkyrie analysis results' screen. Starts at the mentioned screen. </summary>
        /// <param name="expectedFilesNumber">Set an expected files number.</param>
        /// <param name="testContext">Set a current test context.</param>
        private void AssertValkyrieAnalysisResults(int expectedFilesNumber, TestContext testContext)
        {
            int valkyrieResultsRowsNumber = ValkyrieAnalysisResultsTab().GetRows().Length;
            
            if (valkyrieResultsRowsNumber == expectedFilesNumber)
                testContext.WriteLine(String.Format("The total number of files found {0} and the number of files at 'Valkyrie analysis results' screen {1} coincide.\n", expectedFilesNumber, valkyrieResultsRowsNumber));
            else Assert.Fail(String.Format("The total number of files found {0} and the number of files at 'Valkyrie analysis results' screen {1} do not coincide!", expectedFilesNumber, valkyrieResultsRowsNumber));
        }

        /// <summary> Waits until the Valkyrie results are received. </summary>
        /// <param name="testContext">Set a current TestContext.</param>
        private void WaitForValkyrieResults(TestContext testContext)
        {
            int waitTime = Settings.WaitingTime.Default.ValkyrieAnalysisWaitTime;
            UIHelper.WaitForControl(ValkyrieAnalysisResultsTab(), waitTime);
            UIHelper.WaitForControlIsEnabled(ValkyrieAnalysisResultsTab(), waitTime);
            
            // Waits until the Valkyrie upload progress is completed
            var timer = Stopwatch.StartNew();
            for (; ; )
            {
                Playback.Wait(2000);
                if (!MainWindowTitleTextBlock().DisplayText.Contains("%")) break;
                if ((int) timer.ElapsedMilliseconds > waitTime) throw new Exception(String.Format("Valkyrie upload progress is not completed after {0} minutes!", waitTime / 60000));
            }
            timer.Stop();

            // CodedUI for some reasons does not detect a tab content without switching the tabs
            UIMapResultsWindow.GoToScanningResultsTab();
            UIMapResultsWindow.GoToValkyrieAnalysisResultsTab();

            // Waits until the results are actually available at UI
            int treeWaitTime = Settings.WaitingTime.Default.WpfTreeWaitTime;
            timer.Restart();
            for (; ; )
            {
                Playback.Wait(2000);
                try
                {
                    ValkyrieAnalysisResultsTab().GetRows();
                    testContext.WriteLine("Valkyrie results now can be obtained from UI.\n");
                    break;
                }
                catch (Exception) { }
                if ((int)timer.ElapsedMilliseconds > treeWaitTime) throw new Exception(String.Format("Valkyrie results cannot be obtained from UI after {0} seconds!", treeWaitTime / 1000));
            }
            timer.Stop();
        }

        /// <summary> Starts a scan of a given type at indicated endpoint via Network Addresses branch. Starts at main screen. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        /// <param name="scanType">Set a scan type.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private void StartEndpointScan(Computer endpoint, ScanType scanType, TestContext testContext)
        {
            StartEndpointScan(endpoint, scanType, DataHelper.Username, DataHelper.Password, TestContext);
        }

        /// <summary> Starts a scan of a given type at indicated endpoint via Network Addresses branch using set credentials. Starts at main screen. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        /// <param name="scanType">Set a scan type.</param>
        /// <param name="username">Set a username.</param>
        /// <param name="password">Set a password.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private void StartEndpointScan(Computer endpoint, ScanType scanType, string username, string password, TestContext testContext)
        {
            UIMapMain.ClickStartScanningButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            testContext.WriteLine("Start Scanning wizard appeared...\n");
            UIMapStartScanningWizard.ClickNetworkAddressButton();
            
            UIHelper.WaitForControlIsEnabled(QuickScanButton());
            CleanNetworkAddressesList();
            UIMapNetworkAddresses.InputNetworkAddressParams.UINetworkAddressTextBoEditText = endpoint.Name;
            UIMapNetworkAddresses.InputNetworkAddress();
            UIMapNetworkAddresses.ClickAddButton();
            UIMapNetworkAddresses.InputLoginParams.UILoginTextBoxEditText = username;
            UIMapNetworkAddresses.InputLogin();
            UIMapNetworkAddresses.InputPasswordParams.UIPasswordBoxEditSendKeys = Playback.EncryptText(password);
            UIMapNetworkAddresses.InputPassword();

            switch (scanType)
            {
                case ScanType.Quick:
                    UIMapStartScanningWizard.ClickQuickScanButton();
                    break;
                case ScanType.Full:
                    UIMapStartScanningWizard.ClickFullScanButton();
                    break;
                default: throw new Exception(String.Format("Unexpected scan type '{0}'!", scanType.ToString()));
            }
            testContext.WriteLine(String.Format("'{0}' scan started at endpoint '{1}'...", scanType.ToString(), endpoint.Name));
        }

        /// <summary> Waits for Scan Completed window. </summary>
        /// <param name="scanType">Set a scan type.</param>
        private void WaitForScanCompleted(ScanType scanType)
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

        /// <summary> Stops a scan. </summary>
        private void StopScanning(TestContext testContext)
        {
            UIHelper.WaitForControlIsClickable(StopScanningButton());
            UIMapMain.ClickStopScanningButton();
            UIHelper.WaitForControl(YesDialogButton());
            UIMapStopScanWindow.ClickYesButton();
            testContext.WriteLine("The scanning has been stopped.");
        }

        /// <summary>Cleans Network Addresses list</summary>
        private void CleanNetworkAddressesList()
        {

            while (NetworkAddressesList().Items.Count > 0)
            {
                Mouse.Click(NetworkAddressesList().Items[0]);
                UIMapNetworkAddresses useNetworkAddresses = new UIMapNetworkAddresses();
                useNetworkAddresses.ClickRemoveButton();                
            }
        }

        /// <summary> Inputs CAM credentials at the respective popup window. </summary>
        /// <param name="testContext">Set a current TestContext.</param>
        private void InputCamCredentials(TestContext testContext)
        {
            UIHelper.WaitForControlIsEnabled(CamLoginTextBox());
            testContext.WriteLine("CAM Credentials window appeared...\n");
            UIMapCamCredentialsWindow.InputLoginParams.UILoginWatermarkTextBoEditSendKeys = Settings.General.Default.CamLogin;
            UIMapCamCredentialsWindow.InputLogin();
            UIMapCamCredentialsWindow.InputPasswordParams.UIPasswordBoxEditSendKeys = Playback.EncryptText(Settings.General.Default.CamPassword);
            UIMapCamCredentialsWindow.InputPassword();
            UIMapCamCredentialsWindow.ClickOkButton();
        }

        /// <summary> Asserts the counters values for the bottom panel. </summary>
        /// <param name="expectedUnknownCount">Set an expected Unknown counter value.</param>
        /// <param name="expectedMalwareCount">Set an expected Malware counter value.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private void AssertPanelCountersData(int expectedUnknownCount, int expectedMalwareCount, TestContext testContext)
        {
            UIMapMain.AssertUnknownFilesCounterTextBlockTextExpectedValues.UIItem0TextDisplayText = expectedUnknownCount.ToString();
            UIMapMain.AssertUnknownFilesCounterTextBlockText();
            UIMapMain.AssertMalwareFilesCounterTextBlockTextExpectedValues.UIItem0Text1DisplayText = expectedMalwareCount.ToString();
            UIMapMain.AssertMalwareFilesCounterTextBlockText();
            testContext.WriteLine(String.Format("The bottom panel shows '{0}' unknown and '{1}' malicious files.", expectedUnknownCount, expectedMalwareCount));
        }

        #endregion

        #region Test methods

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void EndpointQuickScanAndValkyrieUriCheckTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            SysHelper.InfectForQuickScan(endpoint);
            SysHelper.CopyUnknownFileForQuickScan(endpoint);

            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);

            if (ScanCompletedTextBlock().DisplayText.Contains(Settings.ScanCompleted.Default.ScanCompletedText))
                TestContext.WriteLine(String.Format("Scan Completed window appeared with text containing: '{0}'.\n", Settings.ScanCompleted.Default.ScanCompletedText));
            else Assert.Fail(String.Format("Scan Completed window appeared with text, which does not contain: '{0}'!\n", Settings.ScanCompleted.Default.ScanCompletedText));

            // Gets the number of Unknown files and Malwares found at Scan Completed window
            int expectedFilesNumber = GetFoundFilesNumber(TestContext);

            UIMapScanCompletedWindow.ClickOkButton();

            // In the case no unknown or malicious files found - the analysis is not available
            if (expectedFilesNumber == 0) 
            {
                if (!AnalyseWithValkyrieButton().Enabled) TestContext.WriteLine("No unknown or malicious files found and Valkyrie analysis is not available.");
                else Assert.Fail("No unknown or malicious files found but Valkyrie analysis is available!");
            }
            else
            {
                InputCamCredentials(TestContext);
                
                // Gets the number of rows in 'Valkyrie analysis results' and compares it with the number of files found
                WaitForValkyrieResults(TestContext);
                AssertValkyrieAnalysisResults(expectedFilesNumber, TestContext);

                // Clicks Valkyrie link and checks the respective browser window URI
                string expectedValkyrieUri = Settings.General.Default.ValkyrieUri;
                UIHelper.WaitForControl(ValkyrieLink());
                UIMapMain.ClickValkyrieLink();
                TestContext.WriteLine("Valkyrie link clicked...\n");
                BrowserWindow valkyrieWebPage = new BrowserWindow();
                valkyrieWebPage.SearchProperties.Add(new PropertyExpression(BrowserWindow.PropertyNames.Name, Settings.General.Default.ValkyrieUri, PropertyExpressionOperator.Contains));
                UIHelper.WaitForControl(valkyrieWebPage);
                string actualValkyrieUri = valkyrieWebPage.Uri.ToString();
                string error = null;
                if (actualValkyrieUri.Contains(expectedValkyrieUri))
                    TestContext.WriteLine(String.Format("Browser window opened with a correct address: '{0}'.\n", actualValkyrieUri));
                else error = String.Format("Browser window opened with an incorrect address: '{0}'!", actualValkyrieUri);
                valkyrieWebPage.Close();
                if (!String.IsNullOrEmpty(error)) Assert.Fail(error);
            }
        }

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void WorkgroupQuickScanAndThreatDetailsCheckTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            string workgroupName = SysHelper.GetWorkgroupName();
            Malware malware = SysHelper.InfectForQuickScan(endpoint);
            SysHelper.CopyUnknownFileForQuickScan(endpoint);
            
            UIMapMain.ClickStartScanningNowButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());

            UIMapStartScanningWizard.ClickWorkgroupButton();
            UIHelper.WaitForControlIsEnabled(QuickScanButton());

            UIMapWorkgroup.InputWorkgroupParams.UIWorkgroupTextBoxEditText = workgroupName;
            UIMapWorkgroup.InputWorkgroup();
            UIMapWorkgroup.InputLoginParams.UILoginTextBoxEditText = DataHelper.Username;
            UIMapWorkgroup.InputLogin();
            UIMapWorkgroup.InputPasswordParams.UIPasswordBoxEditSendKeys = DataHelper.EncriptedPassword;
            UIMapWorkgroup.InputPassword();
            
            UIMapStartScanningWizard.ClickQuickScanButton();
            TestContext.WriteLine(String.Format("Quick Scan started in Workgroup '{0}'...\n", workgroupName));

            WaitForScanCompleted(ScanType.Quick);
            int expectedFilesNumber = GetFoundFilesNumber(TestContext);
            UIMapScanCompletedWindow.ClickOkButton();
            
            UIHelper.WaitForControlIsEnabled(CamLoginTextBox());
            TestContext.WriteLine("CAM Credentials window appeared...\n");
            UIMapCamCredentialsWindow.InputCamKeyParams.UICamLicenseKeyWatermaEditSendKeys = DataHelper.GetCamKey();
            UIMapCamCredentialsWindow.InputCamKey();
            UIMapCamCredentialsWindow.ClickOkButton();
            
            // Gets the number of rows in 'Valkyrie analysis results' and compares it with the number of files found
            WaitForValkyrieResults(TestContext);
            AssertValkyrieAnalysisResults(expectedFilesNumber, TestContext);

            // Verifies the scanning result at the Results screen
            UIMapResultsWindow.GoToScanningResultsTab();
            UIHelper.ClickRow(ResultsScreen.ScanningResults, endpoint);
            TestContext.WriteLine(String.Format("Computer '{0}' expanded at 'Unknown files' screen.\n", endpoint.Name));
            UIHelper.ClickRow(ResultsScreen.ScanningResults, malware, MouseClick.DoubleLeft);
            TestContext.WriteLine(String.Format("Malware '{0}' details opened.\n", malware.Location));
            UIHelper.WaitForControl(CloseDetailsButton());
            UIMapDetailsWindow.AssertMalwareNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = Settings.General.Default.ThreatName;
            UIMapDetailsWindow.AssertMalwareNameTextBlockText();
            UIMapDetailsWindow.ClickCloseButton();
        }
       
        [TestMethod]
        public void VerifyMainScreenDefaultsTest()
        {
            CodedUITestHelp useHelp = new CodedUITestHelp();
            
            UIMapMain.ClickResizeButton();
            if (MainWindow().Maximized) TestContext.WriteLine("Main window was successfully maximized using Resize button.\n");
            else Assert.Fail("Main window was not maximized after clicking Resize button!");

            UIMapMain.ClickResizeButton();
            if (!MainWindow().Maximized) TestContext.WriteLine("Main window is not maximized after clicking Resize button again.\n");
            else Assert.Fail("Main window is maximized after clicking Resize button again!");

            UIMapMain.ClickMinimizeButton();
            if (MainWindow().Minimized) TestContext.WriteLine("Main window is successfully minimized after clicking Minimize button.\n");
            else Assert.Fail("Main window is not minimized after clicking Minimize button!");
            MainWindow().Minimized = false;

            if (StartScanningButton().Enabled) TestContext.WriteLine("Start Scanning button is enabled by default.\n");
            else Assert.Fail("Start Scanning button is disabled by default!\n");

            if (!StopScanningButton().Enabled) TestContext.WriteLine("Stop Scanning button is disabled by default.\n");
            else Assert.Fail("Stop Scanning button is enabled by default!\n");

            if (!AnalyseWithValkyrieButton().Enabled) TestContext.WriteLine("'Analyse with Valkyrie' button is disabled by default.\n");
            else Assert.Fail("'Analyse with Valkyrie' button is enabled by default!\n");
            
            if (!ReportsDropDownButton().Enabled) TestContext.WriteLine("Reports button is disabled by default.\n");
            else Assert.Fail("Reports button is enabled by default!\n");

            if (useHelp.HelpButton().Enabled) TestContext.WriteLine("Help button is enabled by default.\n");
            else Assert.Fail("Help button is disabled by default!\n");

            if (!ScanningResultsTab().Exists) TestContext.WriteLine("'Unknown files' list is not shown by default.\n");
            else Assert.Fail("'Unknown files' list is shown by default!\n");

            UIMapMain.AssertNoScansTextBlockTextExpectedValues.UITherearenoscanshavebTextDisplayText = Settings.General.Default.NoScansTextBlockText;
            UIMapMain.AssertNoScansTextBlockText();
            TestContext.WriteLine(String.Format("'{0}' message is shown.\n", Settings.General.Default.NoScansTextBlockText));

            if (StartScanningNowButton().Enabled) TestContext.WriteLine("'Start Scanning Now' button is enabled by default.\n");
            else Assert.Fail("'Start Scanning Now' button is disabled by default!\n");

            UIMapMain.ClickCloseButton();
            if (!MainWindow().Exists) TestContext.WriteLine("Main window was successfully closed using Close button.\n");
            else Assert.Fail("Main window was not closed after clicking Close button!");
        }

        [TestMethod]
        public void StopFullScanTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            string scannerServiceName = Settings.Scanner.Default.ServiceName;
            string scannerPath = String.Format(@"\\{0}\{1}\{2}", endpoint.Name, DataHelper.GetEndpointRoot(true), Settings.Scanner.Default.Directory);

            StartEndpointScan(endpoint, ScanType.Full, TestContext);

            UIHelper.WaitForControlIsEnabled(StopScanningButton());
            UIHelper.WaitForControlIsClickable(StopScanningButton());
            UIMapMain.ClickStopScanningButton();

            // Does not confirm the stop and checks the result
            UIHelper.WaitForControl(YesDialogButton());
            UIMapStopScanWindow.ClickNoButton();

            ScanStatus endpointStatus = endpoint.GetScanStatus();
            if (endpointStatus != ScanStatus.Running) Assert.Fail(String.Format("Computer '{0}' scan status is '{1}' after the scan stop is not confirmed!", endpoint.Name, endpointStatus));
            else TestContext.WriteLine(String.Format("Computer '{0}' scan status is '{1}' after the scan stop is not confirmed.\n", endpoint.Name, endpointStatus));

            if (SysHelper.IsThereService(scannerServiceName, endpoint))
                TestContext.WriteLine(String.Format("Service '{0}' is still present at endpoint '{1}' after a scan stop is not confirmed.\n", scannerServiceName, endpoint.Name));
            else Assert.Fail(String.Format("Service '{0}' is no longer present at endpoint '{1}' after a scan stop is not confirmed!\n", scannerServiceName, endpoint.Name));
            
            // Confirms the stop and checks the result
            StopScanning(TestContext);

            endpointStatus = endpoint.GetScanStatus();
            if (endpointStatus != ScanStatus.Stopped) Assert.Fail(String.Format("Computer '{0}' scan status is '{1}' after the scan stop is confirmed!", endpoint.Name, endpointStatus));
            else TestContext.WriteLine(String.Format("Computer '{0}' scan status is '{1}' after the scan stop is confirmed.\n", endpoint.Name, endpointStatus));

            if (!SysHelper.IsThereService(scannerServiceName, endpoint))
                TestContext.WriteLine(String.Format("Service '{0}' is no longer present at endpoint '{1}' after a scan stop is confirmed.\n", scannerServiceName, endpoint.Name));
            else Assert.Fail(String.Format("Service '{0}' is still present at endpoint '{1}' after a scan stop is confirmed!\n", scannerServiceName, endpoint.Name));

            if (!Directory.Exists(scannerPath))
                TestContext.WriteLine(String.Format("Directory '{0}' is no longer present at endpoint '{1}' after a scan stop is confirmed.\n", scannerPath, endpoint.Name));
            else Assert.Fail(String.Format("Directory '{0}' is still present at endpoint '{1}' after a scan stop is confirmed!\n", scannerPath, endpoint.Name));

            // Checks whether a scan can be started after the stop
            StartEndpointScan(endpoint, ScanType.Full, TestContext);

            endpointStatus = endpoint.GetScanStatus();
            if (endpointStatus != ScanStatus.Running) Assert.Fail(String.Format("Computer '{0}' scan status is '{1}' after another scan start!", endpoint.Name, endpointStatus));
            else TestContext.WriteLine(String.Format("Computer '{0}' scan status is '{1}' after another scan start.\n", endpoint.Name, endpointStatus));

            if (SysHelper.IsThereService(scannerServiceName, endpoint))
                TestContext.WriteLine(String.Format("Service '{0}' is present at endpoint '{1}' after another scan start.\n", scannerServiceName, endpoint.Name));
            else Assert.Fail(String.Format("Service '{0}' is not present at endpoint '{1}' after another scan start!\n", scannerServiceName, endpoint.Name));

            // A cleanup
            StopScanning(TestContext);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Stopped, TestContext);
        }

        [TestMethod]
        public void CheckScannerAgentTest()
        { 
            Computer endpoint = DataHelper.GetEndpoint();
            string procName;

            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Running, TestContext);

            if (SysHelper.IsOs64Bit(endpoint)) procName = Settings.Scanner.Default.ExeName64;
            else procName = Settings.Scanner.Default.ExeName86;

            // Checks whether the scanner process is running
            if (SysHelper.IsThereProcess(procName, endpoint))
                TestContext.WriteLine(String.Format("Process '{0}' is running at endpoint '{1}'.\n", procName, endpoint.Name));
            else Assert.Fail(String.Format("Process '{0}' is not running at endpoint '{1}'!", procName, endpoint.Name));

           // Checks whether there is only one scanner process
           Process[] processes = SysHelper.GetProcess(procName, endpoint);
           if (processes.Length == 1) TestContext.WriteLine(String.Format("There is only one process '{0}' at endpoint '{1}'.\n", procName, endpoint.Name));
           else if (processes.Length == 0) Assert.Fail(String.Format("There are no processes '{0}' at endpoint '{1}'.\n", procName, endpoint.Name));
           else Assert.Fail(String.Format("There is more than one process '{0}' at endpoint '{1}'!", procName, endpoint.Name));

           // Checks scanner files at the endpoint
           string scannerDir = String.Format(@"\\{0}\{1}\{2}", endpoint.Name, DataHelper.GetEndpointRoot(true), Settings.Scanner.Default.Directory);
           var files = Settings.Scanner.Default.TxtFiles;
           files.Add(procName);
           foreach (string file in files)
           {
               string filePath = String.Format(@"{0}\{1}", scannerDir, file);
               if (File.Exists(filePath)) TestContext.WriteLine(String.Format("File '{0}' exists.\n", filePath));
               else Assert.Fail(String.Format("File '{0}' does not exist!", filePath));
           }

           WaitForScanCompleted(ScanType.Quick);

           // Checks whether the scanner process is not running any longer
           if (!SysHelper.IsThereProcess(procName, endpoint))
               TestContext.WriteLine(String.Format("Process '{0}' is not running at endpoint '{1}' after the scan has been completed.\n", procName, endpoint.Name));
           else Assert.Fail(String.Format("Process '{0}' is still running at endpoint '{1}' after the scan has been completed!", procName, endpoint.Name));

           if (!Directory.Exists(scannerDir)) 
               TestContext.WriteLine(String.Format("Directory '{0}' does not exist at endpoint '{1}' after the scan has been completed.\n", scannerDir, endpoint.Name));
           else Assert.Fail(String.Format("Directory '{0}' still exists at endpoint '{1}' after the scan has been completed!", scannerDir, endpoint.Name));
        }

        [TestMethod]
        public void ScanWithInvalidCredentialsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            string correctLogin = DataHelper.Username;
            string correctPassword = DataHelper.Password;
            string invalidLogin = correctLogin + "error";
            string invalidPassword = correctPassword + "error";
            int failedStatusWaitTime = Settings.WaitingTime.Default.ScanStatusFailedWaitTime;

            // Scan with a correct login and invalid password
            StartEndpointScan(endpoint, ScanType.Quick, correctLogin, invalidPassword, TestContext);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Failed, TestContext, failedStatusWaitTime);
            ScanStatus actualScanStatus = UIHelper.GetScanStatus(endpoint);
            if (actualScanStatus != ScanStatus.Failed)
                Assert.Fail(String.Format("Scan is not '{0}' with a correct Username and invalid Password! Actual status is '{1}'!", ScanStatus.Failed, actualScanStatus));
            else TestContext.WriteLine(String.Format("Scan with a correct Username and invalid Password has '{0}' status.", actualScanStatus));

            // Scan with an invalid login and correct password
            StartEndpointScan(endpoint, ScanType.Full, invalidLogin, correctPassword, TestContext);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Failed, TestContext, failedStatusWaitTime);
            actualScanStatus = UIHelper.GetScanStatus(endpoint);
            if (actualScanStatus != ScanStatus.Failed)
                Assert.Fail(String.Format("Scan is not '{0}' with an invalid Username and correct Password! Actual status is '{1}'!", ScanStatus.Failed, actualScanStatus));
            else TestContext.WriteLine(String.Format("Scan with an invalid Username and correct Password has '{0}' status.", actualScanStatus));

            // Scan with an invalid login and invalid password
            StartEndpointScan(endpoint, ScanType.Quick, invalidLogin, invalidPassword, TestContext);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Failed, TestContext, failedStatusWaitTime);
            actualScanStatus = UIHelper.GetScanStatus(endpoint);
            if (actualScanStatus != ScanStatus.Failed)
                Assert.Fail(String.Format("Scan is not '{0}' with an invalid Username and invalid Password! Actual status is '{1}'!", ScanStatus.Failed, actualScanStatus));
            else TestContext.WriteLine(String.Format("Scan with an invalid Username and invalid Password has '{0}' status.", actualScanStatus));
        }

        [TestMethod]
        public void SearchAtResultsScreenTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            Malware malware = SysHelper.InfectForQuickScan(endpoint);
            SysHelper.CopyUnknownFileForQuickScan(endpoint);
            string partialNameInput = endpoint.Name.Substring(1, (int) ((endpoint.Name.Length - 1) / 2));
            string error = "error";
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);

            // Gets the number of Unknown files and Malwares found at Scan Completed window
            int expectedFilesNumber = GetFoundFilesNumber(TestContext);
            UIMapScanCompletedWindow.ClickOkButton();

            InputCamCredentials(TestContext);
            WaitForValkyrieResults(TestContext);

            // Searches with a correct input
            UIMapResultsWindow.ClickGroupByComputersToggleButton();
            UIMapResultsWindow.InputTextToSearchBoxParams.UISearchTextBoxEditText = partialNameInput;
            UIMapResultsWindow.InputTextToSearchBox();
            TestContext.WriteLine("A part of endpoint name is entered to the searchbox at 'Valkyrie analysis results' tab.");
            UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ValkyrieAnalysis, TestContext);
            UIMapResultsWindow.ClickClearSearchBoxButton();

            // Searches with an incorrect input
            UIMapResultsWindow.GoToValkyrieAnalysisResultsTab();
            TestContext.WriteLine("An incorrect search query is entered to the searchbox at 'Valkyrie analysis results' tab.");
            UIMapResultsWindow.InputTextToSearchBoxParams.UISearchTextBoxEditText = partialNameInput + error;
            UIMapResultsWindow.InputTextToSearchBox();
            try
            {
                UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ValkyrieAnalysis, TestContext);
                Assert.Fail(String.Format("Endpoint '{0}' is shown at 'Valkyrie analysis results' tab after an incorrect search query!", endpoint.Name));
            }
            catch (UITestControlNotFoundException)
            {
                TestContext.WriteLine(String.Format("Endpoint '{0}' is not shown at 'Valkyrie analysis results' tab after an incorrect search query.\n", endpoint.Name));
            }
            UIMapResultsWindow.ClickClearSearchBoxButton();
            
            UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ValkyrieAnalysis, TestContext); // The endpoint should appear again

            // Searches with a correct input
            UIMapResultsWindow.GoToScanningResultsTab();
            UIMapResultsWindow.InputTextToSearchBoxParams.UISearchTextBoxEditText = partialNameInput;
            UIMapResultsWindow.InputTextToSearchBox();
            TestContext.WriteLine("A part of endpoint name is entered to the searchbox at 'Scanning results' tab.");
            UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ScanningResults, TestContext);
            UIMapResultsWindow.ClickClearSearchBoxButton();

            // Searches with an incorrect input
            UIMapResultsWindow.GoToScanningResultsTab();
            UIMapResultsWindow.InputTextToSearchBoxParams.UISearchTextBoxEditText = partialNameInput + error;
            UIMapResultsWindow.InputTextToSearchBox();
            TestContext.WriteLine("An incorrect search query is entered to the searchbox at 'Scanning results' tab.");
            try
            {
                UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ScanningResults, TestContext);
                Assert.Fail(String.Format("Endpoint '{0}' is shown on 'Scanning results' tab after an incorrect search query!", endpoint.Name));
            }
            catch (UITestControlNotFoundException)
            {
                TestContext.WriteLine(String.Format("Endpoint '{0}' is not shown on 'Scanning results' tab after an incorrect search query.\n", endpoint.Name));
            }
            UIMapResultsWindow.ClickClearSearchBoxButton();

            UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ScanningResults, TestContext); // The endpoint should appear again
        }

        [TestMethod]
        public void VerifyThreatDetailsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            Malware malware = SysHelper.InfectForQuickScan(endpoint);
            SysHelper.CopyUnknownFileForQuickScan(endpoint);

            // Performs a scan
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickCancelButton();
            UIHelper.WaitForControlIsClickable(StartScanningButton());

            // Opens a threat details via context menu
            UIHelper.ClickRow(ResultsScreen.ScanningResults, endpoint);
            UIHelper.ClickRow(ResultsScreen.ScanningResults, malware);
            UIHelper.ClickRow(ResultsScreen.ScanningResults, malware, MouseClick.Right);
            UIMapResultsWindow.SelectDetailsByComputersContextMenuItem();
            UIHelper.WaitForControl(CloseDetailsButton());

            // Checks all the threat details
            UIMapDetailsWindow.AssertMalwareNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = malware.Name;
            UIMapDetailsWindow.AssertMalwareNameTextBlockText();
            TestContext.WriteLine(String.Format("Threat name is shown correctly: '{0}'.\n", malware.Name));

            UIMapDetailsWindow.AssertFileNameValueTextBlockTextExpectedValues.UIUnknownexeTextDisplayText = malware.Name;
            UIMapDetailsWindow.AssertFileNameValueTextBlockText();
            TestContext.WriteLine(String.Format("Threat 'File Name' parameter is shown correctly: '{0}'.\n", malware.Name));

            UIMapDetailsWindow.AssertVerdictValueTextBlockTextExpectedValues.UIUnknownTextDisplayText = malware.FileVerdict.ToString();
            UIMapDetailsWindow.AssertVerdictValueTextBlockText();
            TestContext.WriteLine(String.Format("Threat 'Verdict' parameter is shown correctly: '{0}'.\n", malware.FileVerdict));

            UIMapDetailsWindow.AssertSizeValueTextBlockTextExpectedValues.UIItem159KBTextDisplayText = malware.DisplaySize;
            UIMapDetailsWindow.AssertSizeValueTextBlockText();
            TestContext.WriteLine(String.Format("Threat 'Size' parameter is shown correctly: '{0}'.\n", malware.DisplaySize));

            UIMapDetailsWindow.AssertCreatedValueTextBlockTextExpectedValues.UIItem66201631837PMTextDisplayText = malware.Created;
            UIMapDetailsWindow.AssertCreatedValueTextBlockText();
            TestContext.WriteLine(String.Format("Threat 'Created' parameter is shown correctly: '{0}'.\n", malware.Created));
             
            UIMapDetailsWindow.AssertModifiedValueTextBlockTextExpectedValues.UIItem66201632241PMTextDisplayText = malware.Modified;
            UIMapDetailsWindow.AssertModifiedValueTextBlockText();
            TestContext.WriteLine(String.Format("Threat 'Modified' parameter is shown correctly: '{0}'.\n", malware.Modified));

            UIMapDetailsWindow.AssertAccessedValueTextBlockTextExpectedValues.UIItem66201631837PMText1DisplayText = malware.Accessed;
            UIMapDetailsWindow.AssertAccessedValueTextBlockText();
            TestContext.WriteLine(String.Format("Threat 'Accessed' parameter is shown correctly: '{0}'.\n", malware.Accessed));

            UIMapDetailsWindow.AssertHashValueTextBlockTextExpectedValues.UIItem2D93186A41CD331ATextDisplayText = malware.Sha1;
            UIMapDetailsWindow.AssertHashValueTextBlockText();
            TestContext.WriteLine(String.Format("Threat 'Hash' parameter is shown correctly: '{0}'.\n", malware.Sha1));

            UIMapDetailsWindow.AssertSignerValueTextBlockTextExpectedValues.UINATextDisplayText = malware.Signer;
            UIMapDetailsWindow.AssertSignerValueTextBlockText();
            TestContext.WriteLine(String.Format("Threat 'Signer' parameter is shown correctly: '{0}'.\n", malware.Signer));

            UIMapDetailsWindow.AssertDescriptionValueTextBlockTextExpectedValues.UIItem7zSFXTextDisplayText = malware.Description;
            UIMapDetailsWindow.AssertDescriptionValueTextBlockText();
            TestContext.WriteLine(String.Format("Threat 'Description' parameter is shown correctly: '{0}'.\n", malware.Description));

            UIMapDetailsWindow.AssertVersionValueTextBlockTextExpectedValues.UIItem920TextDisplayText = malware.Version;
            UIMapDetailsWindow.AssertVersionValueTextBlockText();
            TestContext.WriteLine(String.Format("Threat 'Version' parameter is shown correctly: '{0}'.\n", malware.Version));

            UIMapDetailsWindow.AssertCopyrightValueTextBlockTextExpectedValues.UICopyrightc19992010IgTextDisplayText = malware.Copyright;
            UIMapDetailsWindow.AssertCopyrightValueTextBlockText();
            TestContext.WriteLine(String.Format("Threat 'Copyright' parameter is shown correctly: '{0}'.\n", malware.Copyright));

            UIMapDetailsWindow.AssertProductValueTextBlockTextExpectedValues.UIItem7ZipTextDisplayText = malware.Product;
            UIMapDetailsWindow.AssertProductValueTextBlockText();
            TestContext.WriteLine(String.Format("Threat 'Product' parameter is shown correctly: '{0}'.\n", malware.Product));

            UIMapDetailsWindow.ClickCloseButton();
            UIHelper.WaitForControlIsClickable(StartScanningButton());
        }

        [TestMethod]
        public void CheckScanResultPanelTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();            
            
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            Malware malware = SysHelper.InfectForQuickScan(endpoint);
            TestContext.WriteLine(String.Format("Malware '{0}' is copied to the endpoint '{1}'.", malware.Name, endpoint.Name));
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickOkButton();
            AssertPanelCountersData(0, 1, TestContext);
                        
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            UnknownFile unknown = SysHelper.CopyUnknownFileForQuickScan(endpoint);
            TestContext.WriteLine(String.Format("Unknown file '{0}' is copied to the endpoint '{1}'.", unknown.Name, endpoint.Name));
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickCancelButton();
            AssertPanelCountersData(1, 0, TestContext);
                        
            malware = SysHelper.InfectForQuickScan(endpoint);
            TestContext.WriteLine(String.Format("Unknown file '{0}' is copied to the endpoint '{1}'.", unknown.Name, endpoint.Name));
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickCancelButton();
            AssertPanelCountersData(1, 1, TestContext);
            
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickOkButton();
            AssertPanelCountersData(0, 0, TestContext);
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
