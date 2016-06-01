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
            SysHelper.StartApplication();
            
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

        private WpfButton AboutButton()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIRadToolBarToolBar.UISystemWindowsControlButton;
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
        private int GetFoundFilesNumber(ThreatType fileType, TestContext testContext)
        {
            string searchWord = null;

            switch (fileType)
            {
                case ThreatType.Unknown:
                    searchWord = Settings.ScanCompleted.Default.UnknownFile;
                    break;
                case ThreatType.Malware:
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
            int unknownFilesNumber = GetFoundFilesNumber(ThreatType.Unknown, testContext);
            int malwaresNumber = 0;
            if (ScanStatisticsTextBlock().DisplayText.Contains(Settings.ScanCompleted.Default.Malware))
                malwaresNumber = GetFoundFilesNumber(ThreatType.Malware, testContext);
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
            UIMapMain.ClickStartScanningButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            testContext.WriteLine("Start Scanning wizard appeared...\n");
            UIMapStartScanningWizard.ClickNetworkAddressButton();

            UIHelper.WaitForControlIsEnabled(QuickScanButton());
            UIMapNetworkAddresses.InputNetworkAddressParams.UINetworkAddressTextBoEditText = endpoint.Name;
            UIMapNetworkAddresses.InputNetworkAddress();
            UIMapNetworkAddresses.ClickAddButton();
            UIMapNetworkAddresses.InputLoginParams.UILoginTextBoxEditText = DataHelper.Username;
            UIMapNetworkAddresses.InputLogin();
            UIMapNetworkAddresses.InputPasswordParams.UIPasswordBoxEditSendKeys = DataHelper.EncriptedPassword;
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
            testContext.WriteLine(String.Format("'{0}' scan started at endpoint '{1}'...\n", scanType.ToString(), endpoint.Name));
        }

        /// <summary> Starts a scan of a given type at indicated endpoint via Network Addresses branch using set credentials. Starts at main screen. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        /// <param name="scanType">Set a scan type.</param>
        /// <param name="username">Set a username</param>
        /// <param name="password">Set a password</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private void StartEndpointScan(Computer endpoint, ScanType scanType, string username, string password, TestContext testContext)
        {
            UIMapMain.ClickStartScanningButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            testContext.WriteLine("Start Scanning wizard appeared...");
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

        #endregion

        #region Test methods

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void EndpointQuickScanAndValkyrieUriCheckTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            SysHelper.InfectForQuickScan(endpoint);

            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            
            UIHelper.WaitForControl(ScanCompletedTextBlock(), Settings.WaitingTime.Default.QuickScanWaitTime);
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
                UIHelper.WaitForControlIsEnabled(CamLoginTextBox());
                TestContext.WriteLine("CAM Credentials window appeared...\n");
                UIMapCamCredentialsWindow.InputLoginParams.UILoginWatermarkTextBoEditSendKeys = Settings.General.Default.CamLogin;
                UIMapCamCredentialsWindow.InputLogin();
                UIMapCamCredentialsWindow.InputPasswordParams.UIPasswordBoxEditSendKeys = Playback.EncryptText(Settings.General.Default.CamPassword);
                UIMapCamCredentialsWindow.InputPassword();
                UIMapCamCredentialsWindow.ClickOkButton();
                
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
            //Malware malware = new Malware(String.Format(@"C:\Users\Administrator\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\{0}", Settings.General.Default.ThreatName));
            
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
            
            UIHelper.WaitForControl(ScanCompletedTextBlock(), Settings.WaitingTime.Default.QuickScanWaitTime);
            int expectedFilesNumber = GetFoundFilesNumber(TestContext);
            UIMapScanCompletedWindow.ClickOkButton();
            
            UIHelper.WaitForControlIsEnabled(CamLoginTextBox());
            TestContext.WriteLine("CAM Credentials window appeared...\n");                        
            UIMapCamCredentialsWindow.InputCamKeyParams.UICamLicenseKeyWatermaEditSendKeys = Settings.General.Default.CamKey;
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

            if (AboutButton().Enabled) TestContext.WriteLine("About button is enabled by default.\n");
            else Assert.Fail("About button is disabled by default!\n");

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

            StopScanning(TestContext); // A cleanup
        }

        [TestMethod]
        public void CheckScannerAgentTest()
        { 
            Computer endpoint = DataHelper.GetEndpoint();
            string procName;

            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            Playback.Wait(5000);            
            //UIHelper.WaitForScanStatus(endpoint, ScanStatus.Running);

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

           UIHelper.WaitForControl(ScanCompletedTextBlock(), Settings.WaitingTime.Default.QuickScanWaitTime);

           // Checks whether the scanner process is not running any longer
           if (!SysHelper.IsThereProcess(procName, endpoint))
               TestContext.WriteLine(String.Format("Process '{0}' is not running at endpoint '{1}' after the scan has been completed.\n", procName, endpoint.Name));
           else Assert.Fail(String.Format("Process '{0}' is still running at endpoint '{1}' after the scan has been completed!", procName, endpoint.Name));

           if (!Directory.Exists(scannerDir)) 
               TestContext.WriteLine(String.Format("Directory '{0}' does not exist at endpoint '{1}' after the scan has been completed.\n", scannerDir, endpoint.Name));
           else Assert.Fail(String.Format("Directory '{0}' still exists at endpoint '{1}' after the scan has been completed!", scannerDir, endpoint.Name));
        }

        [TestMethod]
        public void ScanWithInvalidCredentials()
        {

            Computer endpoint = DataHelper.GetEndpoint();
            string correctLogin = DataHelper.Username;
            string correctPassword = DataHelper.Password;
            string invalidLogin = DataHelper.Username + "error";
            string invalidPassword = DataHelper.Password + "error";
            string failedStatusFullMessage = Settings.Results.Default.ScanStatusIndicatorFailedFullMessage;

            // Scan with correct login and invalid password
            StartEndpointScan(endpoint, ScanType.Quick, correctLogin, invalidPassword, TestContext);
            StartScanningButton().WaitForControlEnabled(60000);
            if (UIHelper.GetScanStatus(endpoint) != ScanStatus.Failed)
            {
                Assert.Fail(String.Format("Scan is not '{0}' with correct Username and invalid Password! Actual status is '{1}'.", ScanStatus.Failed, UIHelper.GetScanStatus(endpoint)));
            }
            TestContext.WriteLine(String.Format("Scan with invalid Username and correct Password has '{0}' status.", UIHelper.GetScanStatus(endpoint)));
            if (UIHelper.GetFullScanStatusMessage(endpoint) != failedStatusFullMessage)
                Assert.Fail(String.Format("Scan has incorrect full status message! Expected:'{0}' ; Actual: '{1}'", failedStatusFullMessage, UIHelper.GetFullScanStatusMessage(endpoint)));
            else TestContext.WriteLine("Scan has correct full status message.\n");


            // Scan with invalid login and correct password
            StartEndpointScan(endpoint, ScanType.Full, invalidLogin, correctPassword, TestContext);
            StartScanningButton().WaitForControlEnabled(60000);
            if (UIHelper.GetScanStatus(endpoint) != ScanStatus.Failed)
            {
                Assert.Fail(String.Format("Scan is not '{0}' with invalid Username and correct Password! Actual status is '{1}'.", ScanStatus.Failed, UIHelper.GetScanStatus(endpoint)));
            }
            TestContext.WriteLine(String.Format("Scan with invalid Username and correct Password has '{0}' status.", UIHelper.GetScanStatus(endpoint)));
            if (UIHelper.GetFullScanStatusMessage(endpoint) != failedStatusFullMessage)
                Assert.Fail(String.Format("Scan has incorrect full status message! Expected:'{0}' ; Actual: '{1}'", failedStatusFullMessage, UIHelper.GetFullScanStatusMessage(endpoint)));
            else TestContext.WriteLine("Scan has correct full status message.\n");

            // Scan with invalid login and invalid password
            StartEndpointScan(endpoint, ScanType.Quick, invalidLogin, invalidPassword, TestContext);
            StartScanningButton().WaitForControlEnabled(60000);
            if (UIHelper.GetScanStatus(endpoint) != ScanStatus.Failed)
            {
                Assert.Fail(String.Format("Scan is not '{0}' with invalid Username and invalid Password! Actual status is '{1}'.", ScanStatus.Failed, UIHelper.GetScanStatus(endpoint)));
            }
            TestContext.WriteLine(String.Format("Scan with invalid Username and correct Password has '{0}' status.", UIHelper.GetScanStatus(endpoint)));
            if (UIHelper.GetFullScanStatusMessage(endpoint) != failedStatusFullMessage)
                Assert.Fail(String.Format("Scan has incorrect full status message! Expected:'{0}' ; Actual: '{1}'", failedStatusFullMessage, UIHelper.GetFullScanStatusMessage(endpoint)));
            else TestContext.WriteLine("Scan has correct full status message.\n");
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
