using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
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

namespace CodedUITestAT
{
    /// <summary> Tests for the Main screen. </summary>
    [CodedUITest]
    public class CodedUITestMain
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

        #endregion
        
        #region Initialize and cleanup

        [TestInitialize]
        public void ATTestInitialize()
        {
            SysHelper.StartProcess(DataHelper.GetWorkingFolder(), DataHelper.ProductExeName, String.Empty);
            UIHelper.WaitForControlIsEnabled(IAcceptTermsOfThisAgreementCheckBox());

            UIMapMain.CheckIAcceptTermsOfThisAgreementCheckBox();
            UIMapMain.ClickContinueButton();

            UIHelper.WaitForControlIsEnabled(StartScanningButton());
        }
        
        [TestCleanup]
        public void ATTestCleanup()
        {
            SysHelper.KillProcesses(Settings.General.Default.ATApplicationProcessName);
        }

        #endregion
        
        #region Control methods
        
        #region Main screen

        private WpfButton StartScanningButton()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIRadToolBarToolBar.UIStartScanningButtonButton;
        }

        private WpfButton AnalyseWithValkyrieButton()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIRadToolBarToolBar.UIAnalyseWithValkyrieBButton;
        }

        private WpfCheckBox IAcceptTermsOfThisAgreementCheckBox()
        {
            return UIMapMain.UICOMODOUnknownFileHunWindow.UIIacceptthetermsofthiCheckBox;
        }

        private WpfText MainWindowTitleTextBlock()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIUnknownFileHunterText;
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

        private WpfTabPage UnknownFilesTab()
        {     
            return UIMapResultsWindow.UIUnknownFileHunterWindow.UIResultsScreenCustom.UIResultsTabControlTabList.UIUnknownfilesTabPage;
        }

        private WpfTabPage ValkyrieAnalysisResultsTab()
        {
            return UIMapResultsWindow.UIUnknownFileHunterWindow.UIResultsScreenCustom.UIResultsTabControlTabList.UIValkyrieanalysisresuTabPage;
        }

        private WpfTree UnknownFilesTree()
        {
            WpfTree uft = UIMapResultsWindow.UIUnknownFileHunterWindow.UIScanResultsScreenCustom.UIRadTreeListViewTree;
            uft.Container = UnknownFilesTab();
            return uft;
        }

        private WpfTree ValkyrieResultsTree()
        {
            WpfTree vrt = UIMapResultsWindow.UIUnknownFileHunterWindow.UIValkyrieResultsScreeCustom.UIRadTreeListViewTree;
            vrt.Container = ValkyrieAnalysisResultsTab();
            return vrt;
        }

        #endregion

        #region Details window

        private WpfButton CloseDetailsButton()
        {
            return UIMapDetailsWindow.UIUnknownFileHunterWindow.UIThreatDetailsWindowCustom.UICloseButton;
        }

        #endregion

        #endregion

        #region Local helper methods

        /// <summary> Gets the number of files of a given type from Scan Completed window. Starts at Scan Completed window. </summary>
        /// <param name="fileType">Set a file type.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private int GetFoundFilesNumber(FileType fileType, TestContext testContext)
        {
            string searchWord = null;

            switch (fileType)
            {
                case FileType.Unknown:
                    searchWord = Settings.ScanCompleted.Default.UnknownFiles;
                    break;
                case FileType.Malware:
                    searchWord = Settings.ScanCompleted.Default.Malwares;
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
            int unknownFilesNumber = GetFoundFilesNumber(FileType.Unknown, testContext);
            int malwaresNumber = 0;
            if (ScanStatisticsTextBlock().DisplayText.Contains(Settings.ScanCompleted.Default.Malwares))
                                                            malwaresNumber = GetFoundFilesNumber(FileType.Malware, testContext);
            return unknownFilesNumber + malwaresNumber;
        }

        /// <summary> Asserts the number of analyzed files at 'Valkyrie analysis results' screen. Starts at the mentioned screen. </summary>
        /// <param name="expectedFilesNumber">Set an expected files number.</param>
        /// <param name="testContext">Set a current test context.</param>
        private void AssertValkyrieAnalysisResults(int expectedFilesNumber, TestContext testContext)
        {
            int valkyrieResultsRowsNumber = 0;
            UIMapResultsWindow.GoToUnknownFilesTab();
            UIMapResultsWindow.GoToValkyrieAnalysisResultsTab();

            // Waits while the screen is refreshed
            for (int i = 0; i < 3; i++)
            {
                Playback.Wait(5000);
                valkyrieResultsRowsNumber = ValkyrieAnalysisResultsTab().GetTabRows().Length;
                if (valkyrieResultsRowsNumber != 0) break;
            }
            
            if (valkyrieResultsRowsNumber == expectedFilesNumber)
                testContext.WriteLine(String.Format("The total number of files found {0} and the number of files at 'Valkyrie analysis results' screen {1} coincide.\n", expectedFilesNumber, valkyrieResultsRowsNumber));
            else Assert.Fail(String.Format("The total number of files found {0} and the number of files at 'Valkyrie analysis results' screen {1} do not coincide!", expectedFilesNumber, valkyrieResultsRowsNumber));
        }

        /// <summary> Clicks a given row at a given results screen. Starts at Results screen. </summary>
        ///<param name="screen">Set a results screen.</param>
        /// <param name="rowName">Set a row name.</param>
        private void ClickRow(ResultsScreen screen, string rowName, MouseClick click = MouseClick.Left)
        {
            WpfButton expandRowButton;
            WpfTabPage tab;

            switch (screen)
            {
                case ResultsScreen.Files:
                    UIMapResultsWindow.GoToUnknownFilesTab();
                    tab = UnknownFilesTab();
                    break;
                case ResultsScreen.Analysis:
                    UIMapResultsWindow.GoToValkyrieAnalysisResultsTab();
                    tab = ValkyrieAnalysisResultsTab();
                    break;
                default: throw new Exception("Unexpected Results screen!");
            }
            
            var rows = tab.GetTabRows();

            foreach (var row in rows)
            {
                if (row.GetRowName().Contains(rowName))
                {
                    expandRowButton = row.GetChildren()[0] as WpfButton;
                    if (click == MouseClick.DoubleLeft) Mouse.DoubleClick(expandRowButton);
                    else if (click == MouseClick.Right) Mouse.Click(expandRowButton, MouseButtons.Right);
                    else Mouse.Click(expandRowButton);
                    break;
                }
            }
        }

        /// <summary> Waits until the Valkyrie results are received. </summary>
        /// <param name="testContext">Set a current TestContext.</param>
        private void WaitForValkyrieResults(TestContext testContext)
        {
            int waitTime = Settings.WaitingTime.Default.ValkyrieUploadWaitTime;
            UIHelper.WaitForControl(ValkyrieAnalysisResultsTab(), waitTime);
            UIHelper.WaitForControlIsEnabled(ValkyrieAnalysisResultsTab(), waitTime);
            
            var timer = Stopwatch.StartNew();
            for (; ; )
            {
                Playback.Wait(2000);
                if (!MainWindowTitleTextBlock().DisplayText.Contains("%")) break;
                if ( (int) timer.ElapsedMilliseconds > waitTime )
                    throw new Exception(String.Format("Complete Valkyrie results are not received after {0} minutes!", waitTime / 60000));
            }
            timer.Stop();

            testContext.WriteLine("Complete Valkyrie results received...\n");
        }

        #endregion

        #region Test methods

        [TestMethod]
        public void EndpointQuickScanAndValkyrieUriCheckTest()
        {
            string endpointName = DataHelper.GetEndpointName();
            SysHelper.InfectForQuickScan(endpointName);

            UIMapMain.ClickStartScanningButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            TestContext.WriteLine("Start Scanning wizard appeared...\n");
            UIMapStartScanningWizard.ClickNetworkAddressButton();
            
            UIHelper.WaitForControlIsEnabled(QuickScanButton());
            UIMapNetworkAddresses.InputNetworkAddressParams.UINetworkAddressTextBoEditText = endpointName;
            UIMapNetworkAddresses.InputNetworkAddress();
            UIMapNetworkAddresses.ClickAddButton();
            UIMapNetworkAddresses.InputLoginParams.UILoginTextBoxEditText = DataHelper.Username;
            UIMapNetworkAddresses.InputLogin();
            UIMapNetworkAddresses.InputPasswordParams.UIPasswordBoxEditSendKeys = DataHelper.EncriptedPassword;
            UIMapNetworkAddresses.InputPassword();

            UIHelper.WaitForControlIsClickable(QuickScanButton());
            UIMapStartScanningWizard.ClickQuickScanButton();
            TestContext.WriteLine(String.Format("Quick Scan started at endpoint '{0}'...\n", endpointName));
            
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

        [TestMethod]
        public void WorkgroupQuickScanAndThreatDetailsCheckTest()
        {
            string endpointName = DataHelper.GetEndpointName();
            string workgroupName = SysHelper.GetWorkgroupName();
            string malwarePath = SysHelper.InfectForQuickScan(endpointName);
            //string malwarePath = String.Format(@"Users\Administrator\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\{0}", Settings.General.Default.ThreatName);
            
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
            ClickRow(ResultsScreen.Files, endpointName);
            TestContext.WriteLine(String.Format("Computer '{0}' expanded at 'Unknown files' screen.\n", endpointName));
            ClickRow(ResultsScreen.Files, malwarePath, MouseClick.DoubleLeft);
            TestContext.WriteLine(String.Format("Malware '{0}' details opened.\n", malwarePath));
            UIHelper.WaitForControl(CloseDetailsButton());
            UIMapDetailsWindow.AssertMalwareNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = Settings.General.Default.ThreatName;
            UIMapDetailsWindow.AssertMalwareNameTextBlockText();
            UIMapDetailsWindow.ClickCloseButton();
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
