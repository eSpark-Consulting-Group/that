using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestAT.UIMaps.UIMapMainClasses;
using CodedUITestAT.UIMaps.StartScanWizard.UIMapStartScanWizardClasses;
using CodedUITestAT.UIMaps.StartScanWizard.UIMapNetworkAddressesClasses;
using CodedUITestAT.UIMaps.StartScanWizard.UIMapWorkgroupClasses;
using CodedUITestAT.UIMaps.UIMapScanCompletedWindowClasses;
using CodedUITestAT.UIMaps.UIMapCamCredentialsWindowClasses;
using CodedUITestAT.UIMaps.UIMapResultsWindowClasses;
using CodedUITestAT.UIMaps.Details.UIMapDetailsWindowClasses;
using CodedUITestAT.UIMaps.UIMapStopScanWindowClasses;
using CodedUITestAT.UIMaps.StartScanningWizard.UIMapActiveDirectoryClasses;
using CodedUITestAT.UIMaps.Details.UIMapValkyrieDetailsWindowClasses;

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
            useEula.UIMapEula.ClickIAcceptButton();

            UIHelper.WaitForControlIsEnabled(StartScanButton());
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
        
        #region Main screen

        internal WpfWindow MainWindow()
        {
            return UIMapMain.UIUnknownFileHunterWindow;
        }
        
        internal WpfButton StartScanButton()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIRadToolBarToolBar.UIStartScanningButtonButton;
        }

        private WpfButton ScanNowButton()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UISendingFilesforAnalyProgressBar.UIStartScanningNowButton;
        }

        private WpfButton StopScanButton()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIRadToolBarToolBar.UIStopScanningButtonButton;
        }

        private WpfButton AnalyzeWithValkyrieButton()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIRadToolBarToolBar.UIAnalyseWithValkyrieBButton;
        }

        private WpfButton ReportsDropDownButton()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIRadToolBarToolBar.UIReportButtonButton;
        }

        private WpfText MainWindowTitleTextBlock()
        {
            return UIMapMain.UIUnknownFileHunterWindow1.UIUnknownFileHunterText;
        }

        private WpfHyperlink ValkyrieLink()
        {
            return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UIPleaseclickheretoseeText.UIPleaseclickheretoseeHyperlink;
        }

        private WpfEdit SearchBox()
        {
          return UIMapMain.UIUnknownFileHunterWindow.UIMainViewxamlPane.UISearchTextBoxEdit;
        }

        #endregion

        #region Start Scan Wizard

        private WpfButton NetworkAddressButton()
        {
            return UIMapStartScanWizard.UIUnknownFileHunterWindow1.UIScanningWizardWindowCustom.UILoadingProgressBar.UIScancomputersbyspeciButton;
        }

        private WpfButton QuickScanButton()
        {
            return UIMapStartScanWizard.UIUnknownFileHunterWindow.UIScanningWizardWindowCustom.UIQuickScanButton;
        }        

        private WpfButton NextButton()
        {
            return UIMapStartScanWizard.UIUnknownFileHunterWindow.UIScanningWizardWindowCustom.UINextButton;
        }

        #region Active Directory

        private WpfTree AdTree()
        {
           return UIMapActiveDirectory.UIUnknownFileHunterWindow.UIScanningWizardWindowCustom.UIActiveDirectoryViewCustom.UIAdTreeTree;
        }

        #endregion

        #region Network Addresses

        private WpfList NetworkAddressesList()
        {
            return UIMapNetworkAddresses.UIUnknownFileHunterWindow.UINetworkAddressesViewCustom.UITextEntryListViewCustom.UINetworkAddressItemsLList;
        }

        private WpfText ErrorTextBlock()
        {
            return UIMapStartScanWizard.UIUnknownFileHunterWindow.UIScanningWizardWindowCustom.UIPleaseSpecifyPassworText;
        }

        #endregion

        #region Workgroup

        private WpfTreeItem WorkgroupTreeItem(string workgroupName)
        {
            UIMapWorkgroup.UIUnknownFileHunterWindow1.UIWorkgroupViewCustom.UIWorkgroupTreeTree.UIWorkgroupTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name]
                        = workgroupName;
            return UIMapWorkgroup.UIUnknownFileHunterWindow1.UIWorkgroupViewCustom.UIWorkgroupTreeTree.UIWorkgroupTreeItem;
        }

        #endregion

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

        internal WpfTabPage ScanResultsTab()
        {     
            return UIMapResultsWindow.UIUnknownFileHunterWindow.UIResultsScreenCustom.UIResultsTabControlTabList.UIUnknownfilesTabPage;
        }

        internal WpfTabPage ValkyrieAnalysisResultsTab()
        {
            return UIMapResultsWindow.UIUnknownFileHunterWindow.UIResultsScreenCustom.UIResultsTabControlTabList.UIValkyrieanalysisresuTabPage;
        }

        internal WpfTable ScanningResultsTable()
        {            
            return UIMapResultsWindow.UIUnknownFileHunterWindow1.UIScanResultsScreenByCCustom.UIRadTreeListViewTable;
        }

        private WpfText SendingFilesTextBlock()
        {
            return UIMapResultsWindow.UIUnknownFileHunterWindow1.UIViewsMainViewxamlPane.UISendingFilesforAnalyProgressBar.UISendingFilesforAnalyText;
        }

        #endregion

        #region Details window

        private WpfButton CloseDetailsButton()
        {
            return UIMapDetailsWindow.UIUnknownFileHunterWindow.UIThreatDetailsWindowCustom.UICloseButton;
        }

        #endregion

        #region Valkyrie Details window

        private WpfButton CloseValkyrieDetailsButton()
        {
            return UIMapValkyrieDetailsWindow.UIUnknownFileHunterWindow.UIValkyrieDetailsWindoCustom.UICloseButton;
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
                testContext.WriteLine(String.Format("'{0}' files number {1} successfully found at Scan Completed window.", searchWord, filesNumber));
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
                testContext.WriteLine(String.Format("The total number of files found {0} and the number of files at 'Valkyrie analysis results' screen {1} coincide.", expectedFilesNumber, valkyrieResultsRowsNumber));
            else Assert.Fail(String.Format("The total number of files found {0} and the number of files at 'Valkyrie analysis results' screen {1} do not coincide!", expectedFilesNumber, valkyrieResultsRowsNumber));
        }

        /// <summary> Waits until the Valkyrie results are received. </summary>
        /// <param name="testContext">Set a current TestContext.</param>
        internal void WaitForValkyrieResults(TestContext testContext)
        {
            int waitTime = Settings.WaitingTime.Default.ValkyrieAnalysisWaitTime;
            UIHelper.WaitForControl(ValkyrieAnalysisResultsTab(), waitTime);
            UIHelper.WaitForControlIsEnabled(ValkyrieAnalysisResultsTab(), waitTime);

            // Waits until the Valkyrie upload progress is completed
            UIHelper.WaitForControlIsClickable(SendingFilesTextBlock(), waitTime, false);
            var timer = Stopwatch.StartNew();
            for (; ; )
            {
                Playback.Wait(2000);
                if (!MainWindowTitleTextBlock().DisplayText.Contains("%")) break;
                if ((int) timer.ElapsedMilliseconds > waitTime) throw new Exception(String.Format("Valkyrie upload progress is not completed after {0} minutes!", waitTime / 60000));
            }
            timer.Stop();

            // CodedUI for some reasons does not detect a tab content without switching/refreshing the tabs
            UIMapResultsWindow.GoToScanResultsTab();
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
                    testContext.WriteLine("Valkyrie results now can be obtained from UI.");
                    break;
                }
                catch (Exception) { }
                if ((int) timer.ElapsedMilliseconds > treeWaitTime) throw new Exception(String.Format("Valkyrie results cannot be obtained from UI after {0} seconds!", treeWaitTime / 1000));
            }
            timer.Stop();
        }

        /// <summary> Starts a scan of a given type at indicated endpoints via Network Addresses branch using set credentials. Starts at Main screen. </summary>        
        /// <param name="scanType">Set a scan type.</param>
        /// <param name="username">Set a username.</param>
        /// <param name="password">Set a password.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        /// <param name="endpoints">Set endpoints.</param>
        private void StartEndpointScan(ScanType scanType, string username, string password, TestContext testContext, params Computer[] endpoints)
        {
            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            testContext.WriteLine("Start Scan Wizard appeared...");
            UIMapStartScanWizard.ClickNetworkAddressButton();

            UIHelper.WaitForControlIsEnabled(NextButton());
            CleanNetworkAddressesList();
            foreach (var endpoint in endpoints)
            {
                UIMapNetworkAddresses.InputNetworkAddressParams.UINetworkAddressTextBoEditText = endpoint.Name;
                UIMapNetworkAddresses.InputNetworkAddress();
                UIMapNetworkAddresses.ClickAddButton();
            }
            UIMapStartScanWizard.ClickNextButton();
            InputScanLogin(username);
            InputPassword(password);

            switch (scanType)
            {
                case ScanType.Quick:
                    UIMapStartScanWizard.ClickQuickScanButton();
                    break;
                case ScanType.Full:
                    UIMapStartScanWizard.ClickFullScanButton();
                    break;
                default: throw new Exception(String.Format("Unexpected scan type '{0}'!", scanType.ToString()));
            }
            
            foreach (var endpoint in endpoints) testContext.WriteLine(String.Format("'{0}' scan started at endpoint '{1}'...", scanType.ToString(), endpoint.Name));
        }

        /// <summary> Starts a scan of a given type at indicated endpoint via Network Addresses branch. Starts at Main screen. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        /// <param name="scanType">Set a scan type.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        internal void StartEndpointScan(Computer endpoint, ScanType scanType, TestContext testContext)
        {
            StartEndpointScan(scanType, DataHelper.Username, DataHelper.Password, testContext, endpoint);
        }

        /// <summary> Starts a scan of a given type at indicated endpoints via Network Addresses branch. Starts at Main screen. </summary>        
        /// <param name="scanType">Set a scan type.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        /// <param name="endpoints">Set endpoints.</param>
        internal void StartEndpointScan(ScanType scanType, TestContext testContext, params Computer[] endpoints)
        {
            StartEndpointScan(scanType, DataHelper.Username, DataHelper.Password, testContext, endpoints);
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

            UIHelper.WaitForControl(ScanCompletedTextBlock(), waitTime);
        }

        /// <summary> Stops a scan. </summary>
        private void StopScanning(TestContext testContext)
        {
            UIHelper.WaitForControlIsClickable(StopScanButton());
            UIMapMain.ClickStopScanButton();
            UIHelper.WaitForControl(YesDialogButton());
            UIMapStopScanWindow.ClickYesButton();
            testContext.WriteLine("The scanning has been canceled.");
        }

        /// <summary> Cleans Network Addresses list. </summary>
        private void CleanNetworkAddressesList()
        {
            while (NetworkAddressesList().Items.Count > 0)
            {
              Mouse.Click(NetworkAddressesList().Items[0]);
              UIMapNetworkAddresses.ClickRemoveButton();
            }
        }

        /// <summary> Inputs CAM credentials at the respective popup window. </summary>
        /// <param name="testContext">Set a current TestContext.</param>
        internal void InputCamCredentials(TestContext testContext)
        {
            WpfEdit camLoginTextBox = CamLoginTextBox();
            UIHelper.WaitForControlIsEnabled(camLoginTextBox);
            testContext.WriteLine("'Submit to Valkyrie' window appeared...");
            UIMapCamCredentialsWindow.InputLoginParams.UILoginWatermarkTextBoEditSendKeys = Settings.General.Default.CamLogin;
            if (camLoginTextBox.Text.Contains(" ")) UIMapCamCredentialsWindow.InputLogin();
            UIMapCamCredentialsWindow.InputPasswordParams.UIPasswordWatermarkTexEditSendKeys = Settings.General.Default.CamPassword;
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
            UIMapMain.AssertMalwaresCounterTextBlockTextExpectedValues.UIItem0Text1DisplayText = expectedMalwareCount.ToString();
            UIMapMain.AssertMalwaresCounterTextBlockText();
            testContext.WriteLine(String.Format("The bottom panel shows '{0}' unknown and '{1}' malicious files.", expectedUnknownCount, expectedMalwareCount));
        }

        /// <summary> Expands a given row in Active Directory tree. In accordance with DeploymentItem.cs. </summary>
        /// <param name="rowName">Set a unique row name.</param>
        private void ExpandAdRow(string rowName)
        {
            UIMapActiveDirectory.UIUnknownFileHunterWindow.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDCdevTreeItem.
                                                                                            SearchProperties[WpfTreeItem.PropertyNames.Name] = GetAdRootRowName();
            UIMapActiveDirectory.UIUnknownFileHunterWindow.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDCdevTreeItem.UIComputersTreeItem.
                                                                                            SearchProperties[WpfTreeItem.PropertyNames.Name] = rowName;
            UIMapActiveDirectory.ExpandAdRow();
        }

        /// <summary> Checks a given row in Active Directory tree. In accordance with DeploymentItem.cs. </summary>
        /// <param name="rowName">Set a unique row name.</param>
        private void CheckAdRow(string rowName)
        {
            UIMapActiveDirectory.UIUnknownFileHunterWindow.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDCdevTreeItem.
                                                                                            SearchProperties[WpfTreeItem.PropertyNames.Name] = GetAdRootRowName();
            UIMapActiveDirectory.UIUnknownFileHunterWindow.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDCdevTreeItem.UIComputersTreeItem.
                                                                                            SearchProperties[WpfTreeItem.PropertyNames.Name] = rowName;
            UIMapActiveDirectory.CheckAdRow();
        }

        /// <summary> Gets root (domain controller) row name for Active Directory tree. </summary>
        private string GetAdRootRowName()
        {
            return "DC=" + SysHelper.GetDomainName(true);
        }

        /// <summary> Checks a given workgroup in the workgroups tree. </summary>
        /// <param name="workgroupName">Set a workgroup name.</param>
        private void CheckWorkgroup(string workgroupName)
        {
            UIMapWorkgroup.UIUnknownFileHunterWindow1.UIWorkgroupViewCustom.UIWorkgroupTreeTree.UIWorkgroupTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name] 
                        = workgroupName;
            UIMapWorkgroup.CheckWorkgroup();
        }

        /// <summary> Checks whether the given data are stored at Network Addresses wizard branch. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        /// <param name="login">Set a login.</param>
        /// <param name="tcpPort">Set a TCP port.</param>
        /// <param name="udpPort">Set a UDP port.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private void CheckStoredDataAtNetworkAddresses(Computer endpoint, string login, string tcpPort, string udpPort, TestContext testContext)
        {
            // Network address
            var networkAddresses = NetworkAddressesList().Items;
            bool thereIsNoEndpointName = true;
            foreach (var networkAddress in networkAddresses)
            {
                if (networkAddress.Name == endpoint.Name)
                {
                    testContext.WriteLine(String.Format("Endpoint name '{0}' is still present in Network Addresses list.", endpoint.Name));
                    thereIsNoEndpointName = false;
                    break;
                }
            }
            if (thereIsNoEndpointName) Assert.Fail(String.Format("Endpoint name '{0}' is no longer present in Network Addresses list!", endpoint.Name));

            // Login
            UIMapNetworkAddresses.AssertLoginTextBoxTextExpectedValues.UILoginTextBoxEditText = login;
            UIMapNetworkAddresses.AssertLoginTextBoxText();
            testContext.WriteLine(String.Format("Login '{0}' is stored in the respective text box.", login));
            
            // FLS Ports
            UIMapNetworkAddresses.AssertFlsTcpPortTextBoxTextExpectedValues.UITcpPortTextBoxEditText = tcpPort;
            UIMapNetworkAddresses.AssertFlsTcpPortTextBoxText();
            testContext.WriteLine(String.Format("FLS TCP Port '{0}' is stored in the respective text box.", tcpPort));
            UIMapNetworkAddresses.AssertFlsUdpPortTextBoxTextExpectedValues.UIUdpPortTextBoxEditText = udpPort;
            UIMapNetworkAddresses.AssertFlsUdpPortTextBoxText();
            testContext.WriteLine(String.Format("FLS UDP Port '{0}' is stored in the respective text box.", udpPort));
            
            // Password
            UIMapStartScanWizard.ClickQuickScanButton();
            testContext.WriteLine("Quick button clicked...");
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, testContext);
        }

        /// <summary> Checks whether the given data are stored at Workgroup wizard branch. </summary>
        /// <param name="workgroupName">Set a workgroup name.</param>
        /// <param name="login">Set a login.</param>
        /// <param name="tcpPort">Set a TCP port.</param>
        /// <param name="udpPort">Set a UDP port.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private void CheckStoredDataAtWorkgroup(string workgroupName, string login, string tcpPort, string udpPort, TestContext testContext)
        {
            // Workgroup
            UIMapWorkgroup.AssertWorkgroupTextBoxTextExpectedValues.UIWorkgroupTextBoxEditText = workgroupName;
            UIMapWorkgroup.AssertWorkgroupTextBoxText();
            testContext.WriteLine(String.Format("Workgroup name '{0}' is stored in the respective text box.", workgroupName));
            
            // Login
            UIMapWorkgroup.AssertLoginTextBoxTextExpectedValues.UILoginTextBoxEditText = login;
            UIMapWorkgroup.AssertLoginTextBoxText();
            testContext.WriteLine(String.Format("Login '{0}' is stored in the respective text box.", login));
            
            // FLS Ports
            UIMapWorkgroup.AssertTcpPortTextBoxTextExpectedValues.UITcpPortTextBoxEditText = tcpPort;
            UIMapWorkgroup.AssertTcpPortTextBoxText();
            testContext.WriteLine(String.Format("TCP Port '{0}' is stored in the respective text box.", tcpPort));
            UIMapWorkgroup.AssertUdpPortTextBoxTextExpectedValues.UIUdpPortTextBoxEditText = udpPort;
            UIMapWorkgroup.AssertUdpPortTextBoxText();
            testContext.WriteLine(String.Format("UDP Port '{0}' is stored in the respective text box.", udpPort));
            
            // Password
            UIMapStartScanWizard.ClickQuickScanButton();
            testContext.WriteLine("Quick button clicked...");
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, testContext);
        }

        /// <summary> Checks whether the given data are stored at Active Directory wizard branch. </summary>
        /// <param name="domainName">Set a workgroup name.</param>
        /// <param name="login">Set a login.</param>
        /// <param name="tcpPort">Set a TCP port.</param>
        /// <param name="udpPort">Set a UDP port.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private void CheckStoredDataAtActiveDirectory(string domainName, string login, string tcpPort, string udpPort, TestContext testContext)
        {
            // Domain Controller
            UIMapActiveDirectory.AssertDomainControllerTextBoxTextExpectedValues.UIDomainControllerTextEditText = domainName;
            UIMapActiveDirectory.AssertDomainControllerTextBoxText();
            testContext.WriteLine(String.Format("Domain Controller '{0}' is stored in the respective text box.", domainName));

            // Administrator
            UIMapActiveDirectory.AssertAdministratorTextBoxTextExpectedValues.UIAdministratorTextBoxEditText = login;
            UIMapActiveDirectory.AssertAdministratorTextBoxText();
            testContext.WriteLine(String.Format("Administrator '{0}' is stored in the respective text box.", login));

            // FLS Ports
            UIMapActiveDirectory.AssertTcpPortTextBoxTextExpectedValues.UIFlsTcpPortTextBoxEditText = tcpPort;
            UIMapActiveDirectory.AssertTcpPortTextBoxText();
            testContext.WriteLine(String.Format("TCP Port '{0}' is stored in the respective text box.", tcpPort));
            UIMapActiveDirectory.AssertUdpPortTextBoxTextExpectedValues.UIFlsUdpPortTextBoxEditText = udpPort;
            UIMapActiveDirectory.AssertUdpPortTextBoxText();
            testContext.WriteLine(String.Format("UDP Port '{0}' is stored in the respective text box.", udpPort));

            // Password
            UIMapStartScanWizard.ClickNextButton();
            testContext.WriteLine("Next button clicked...");
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, testContext);
        }

        /// <summary> Asserts a text of regular error message. </summary>
        /// <param name="expectedErrorText">Set expected error text.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        private void AssertErrorText(string expectedErrorText, TestContext testContext)
        {
            UIHelper.WaitForControlIsClickable(ErrorTextBlock());
            UIMapStartScanWizard.AssertErrorTextBlockTextExpectedValues.UIPleaseSpecifyPassworTextDisplayText = expectedErrorText;
            UIMapStartScanWizard.AssertErrorTextBlockText();
            testContext.WriteLine(String.Format("Expected regular error message '{0}' appeared.", expectedErrorText));
        }

        /// <summary> Refreshes the results screen using Search box. </summary>
        private void RefreshResultsScreen()
        {
            UIMapMain.InputTextToSearchBoxParams.UISearchWatermarkTextBEditSendKeys = Guid.NewGuid().ToString();
            UIMapMain.InputTextToSearchBox();
            Playback.Wait(2000);
            UIMapMain.ClickClearSearchTextButton();
            Playback.Wait(2000);
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

        /// <summary> Inputs a password. </summary>
        /// <param name="password">Set a password to input.</param>
        private void InputPassword(string password)
        {
            WpfEdit passwordEdit = UIMapNetworkAddresses.UIUnknownFileHunterWindow1.UIThisCustom.UIPasswordWatermarkTexEdit;
            if (passwordEdit.Text == Settings.General.Default.EmptyWatermarkTextBoxText)
            {                
                UIMapNetworkAddresses.InputPasswordParams.UIPasswordWatermarkTexEditSendKeys = password.Remove(1);
                UIMapNetworkAddresses.InputPasswordParams.UIPasswordBoxEditSendKeys = Playback.EncryptText(password.Substring(1));
                UIMapNetworkAddresses.InputPassword();
            }
        }

        /// <summary> Inputs a login. </summary>
        /// <param name="login">Set a login to input.</param>
        private void InputScanLogin(string login)
        {
            WpfEdit loginEdit = UIMapNetworkAddresses.UIUnknownFileHunterWindow1.UIThisCustom.UILoginWatermarkTextBoEdit;
            if (loginEdit.Text == Settings.General.Default.EmptyWatermarkTextBoxText || loginEdit.Text != login) loginEdit.Text = login;
        }

        #endregion

        #region Test methods
        
        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void EndpointQuickScanAndValkyrieUriCheckTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            SysHelper.InfectForScan(endpoint);
            SysHelper.AddUnknownFileForScan(endpoint);

            StartEndpointScan(endpoint, ScanType.Quick, TestContext);

            WaitForScanCompleted(ScanType.Quick);

            if (ScanCompletedTextBlock().DisplayText.Contains(Settings.ScanCompleted.Default.ScanCompletedText))
                TestContext.WriteLine(String.Format("Scan Completed window appeared with text containing: '{0}'.", Settings.ScanCompleted.Default.ScanCompletedText));
            else Assert.Fail(String.Format("Scan Completed window appeared with text, which does not contain: '{0}'!", Settings.ScanCompleted.Default.ScanCompletedText));

            // Gets the number of Unknown and Malicious files found at Scan Completed window
            int expectedFilesNumber = GetFoundFilesNumber(TestContext);
            UIMapScanCompletedWindow.ClickYesButton();

            // In the case no unknown or malicious files found - the analysis is not available
            if (expectedFilesNumber == 0) 
            {
                if (!AnalyzeWithValkyrieButton().Enabled) TestContext.WriteLine("No unknown or malicious files found and Valkyrie analysis is not available.");
                else Assert.Fail("No unknown or malicious files found but Valkyrie analysis is available!");
            }
            else
            {
                InputCamCredentials(TestContext);
                
                // Gets the number of rows in 'Valkyrie analysis results' and compares it with the number of files found
                WaitForValkyrieResults(TestContext);
                UIHelper.GroupByFiles();
                AssertValkyrieAnalysisResults(expectedFilesNumber, TestContext);
                UIHelper.GroupByComputers();

                // Clicks Valkyrie link and checks the respective browser window URI
                string expectedValkyrieUri = Settings.General.Default.ValkyrieUri;
                UIHelper.WaitForControl(ValkyrieLink());
                UIMapMain.ClickValkyrieLink();
                TestContext.WriteLine("Valkyrie link clicked...");
                UIHelper.AssertBrowserUri(expectedValkyrieUri, TestContext);
            }
        }

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void WorkgroupQuickScanAndThreatDetailsCheckTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            string workgroupName = SysHelper.GetWorkgroupName();
            Malware malware = SysHelper.InfectForScan(endpoint);
            SysHelper.AddUnknownFileForScan(endpoint);
            
            UIMapMain.ClickScanNowButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControlIsEnabled(NextButton());
            UIMapWorkgroup.SelectUseKnownWorkgroupRadioButton();
            UIMapWorkgroup.InputWorkgroupParams.UIWorkgroupWatermarkTeEditSendKeys = workgroupName;
            UIMapWorkgroup.InputWorkgroup();
            UIMapStartScanWizard.ClickNextButton();
            UIHelper.WaitForControl(WorkgroupTreeItem(workgroupName), Settings.WaitingTime.Default.WorkgroupDiscoveryWaitTime);
            UIHelper.WaitForControlIsEnabled(WorkgroupTreeItem(workgroupName));
            CheckWorkgroup(workgroupName);
            UIMapStartScanWizard.ClickNextButton();
            InputScanLogin(DataHelper.Username);
            InputPassword(DataHelper.Password);
            UIMapStartScanWizard.ClickQuickScanButton();
            TestContext.WriteLine(String.Format("Quick Scan started in Workgroup '{0}'...", workgroupName));

            WaitForScanCompleted(ScanType.Quick);
            int expectedFilesNumber = GetFoundFilesNumber(TestContext);
            UIMapScanCompletedWindow.ClickYesButton();
            
            UIHelper.WaitForControlIsEnabled(CamLoginTextBox());
            TestContext.WriteLine("'Submit to Valkyrie' window appeared...");
            UIMapCamCredentialsWindow.InputCamKeyParams.UICamLicenseKeyWatermaEditSendKeys = DataHelper.GetCamKey();
            UIMapCamCredentialsWindow.InputCamKey();
            UIMapCamCredentialsWindow.ClickOkButton();
            
            // Gets the number of rows in 'Valkyrie analysis results' and compares it with the number of files found
            WaitForValkyrieResults(TestContext);
            UIHelper.GroupByFiles();
            AssertValkyrieAnalysisResults(expectedFilesNumber, TestContext);

            // Verifies the scanning result at the Results screen
            UIMapResultsWindow.GoToScanResultsTab();
            UIHelper.GroupByComputers();
            UIHelper.ClickRow(ResultsScreen.ScanResults, endpoint);
            TestContext.WriteLine(String.Format("Computer '{0}' expanded at 'Scan results' screen.", endpoint.Name));
            UIHelper.ClickRow(ResultsScreen.ScanResults, malware, MouseClick.DoubleLeft);
            TestContext.WriteLine(String.Format("Malware '{0}' details opened.", malware.Location));
            UIHelper.WaitForControl(CloseDetailsButton());
            UIMapDetailsWindow.AssertThreatNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = malware.Name;
            UIMapDetailsWindow.AssertThreatNameTextBlockText();
            UIMapDetailsWindow.ClickCloseButton();
        }
       
        [TestMethod]
        public void VerifyMainScreenDefaultsTest()
        {
            CodedUITestHelp useHelp = new CodedUITestHelp();
            
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

            if (StartScanButton().Enabled) TestContext.WriteLine("Start Scan button is enabled by default.");
            else Assert.Fail("Start Scan button is disabled by default!");

            if (!StopScanButton().Enabled) TestContext.WriteLine("Stop Scan button is disabled by default.");
            else Assert.Fail("Stop Scan button is enabled by default!");

            if (!AnalyzeWithValkyrieButton().Enabled) TestContext.WriteLine("'Analyze with Valkyrie' button is disabled by default.");
            else Assert.Fail("'Analyse with Valkyrie' button is enabled by default!");
            
            if (!ReportsDropDownButton().Enabled) TestContext.WriteLine("Reports button is disabled by default.\n");
            else Assert.Fail("Reports button is enabled by default!");

            if (useHelp.HelpButton().Enabled) TestContext.WriteLine("Help button is enabled by default.");
            else Assert.Fail("Help button is disabled by default!");

            if (!ScanResultsTab().Exists) TestContext.WriteLine("'Scan results' list is not shown by default.");
            else Assert.Fail("'Scan results' list is shown by default!");

            UIMapMain.AssertNoScansTextBlockTextExpectedValues.UITherearenoscanshavebTextDisplayText = Settings.General.Default.NoScansTextBlockText;
            UIMapMain.AssertNoScansTextBlockText();
            TestContext.WriteLine(String.Format("'{0}' text is shown.", Settings.General.Default.NoScansTextBlockText));

            if (ScanNowButton().Enabled) TestContext.WriteLine("'Scan Now' button is enabled by default.");
            else Assert.Fail("'Scan Now' button is disabled by default!");

            UIMapMain.AssertComodoOneReferralTextBlockTextExpectedValues.UIThisproductisjustoneTextDisplayText = Settings.General.Default.ComodoOneReferralText;
            UIMapMain.AssertComodoOneReferralTextBlockText();
            TestContext.WriteLine(String.Format("'{0}' text is shown.", Settings.General.Default.ComodoOneReferralText));

            UIMapMain.ClickComodoOneReferralLink();
            UIHelper.AssertBrowserUri(Settings.General.Default.ComodoOneReferralUri, TestContext);

            UIMapMain.ClickCloseButton();
            if (!MainWindow().Exists) TestContext.WriteLine("Main window was successfully closed using Close button.");
            else Assert.Fail("Main window was not closed after clicking Close button!");
        }

        [TestMethod]
        public void StopFullScanTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            string scannerServiceName = Settings.Scanner.Default.ServiceName;
            string scannerPath = String.Format(@"\\{0}\{1}\{2}", endpoint.Name, DataHelper.GetEndpointRoot(true), Settings.Scanner.Default.Directory);

            StartEndpointScan(endpoint, ScanType.Full, TestContext);

            UIHelper.WaitForControlIsEnabled(StopScanButton());
            UIHelper.WaitForControlIsClickable(StopScanButton());
            UIMapMain.ClickStopScanButton();

            // Does not confirm the stop and checks the result
            UIHelper.WaitForControl(YesDialogButton());
            UIMapStopScanWindow.ClickNoButton();

            ScanStatus endpointStatus = endpoint.GetScanStatus();
            if (endpointStatus != ScanStatus.InProgress) Assert.Fail(String.Format("Computer '{0}' scan status is '{1}' after the scan stop is not confirmed!", endpoint.Name, endpointStatus));
            else TestContext.WriteLine(String.Format("Computer '{0}' scan status is '{1}' after the scan stop is not confirmed.", endpoint.Name, endpointStatus));

            if (SysHelper.IsThereService(scannerServiceName, endpoint))
                TestContext.WriteLine(String.Format("Service '{0}' is still present at endpoint '{1}' after a scan stop is not confirmed.", scannerServiceName, endpoint.Name));
            else Assert.Fail(String.Format("Service '{0}' is no longer present at endpoint '{1}' after a scan stop is not confirmed!", scannerServiceName, endpoint.Name));
            
            // Confirms the stop and checks the result
            StopScanning(TestContext);

            endpointStatus = endpoint.GetScanStatus();
            if (endpointStatus != ScanStatus.Canceled) Assert.Fail(String.Format("Computer '{0}' scan status is '{1}' after the scan stop is confirmed!", endpoint.Name, endpointStatus));
            else TestContext.WriteLine(String.Format("Computer '{0}' scan status is '{1}' after the scan stop is confirmed.", endpoint.Name, endpointStatus));

            if (!SysHelper.IsThereService(scannerServiceName, endpoint))
                TestContext.WriteLine(String.Format("Service '{0}' is no longer present at endpoint '{1}' after a scan stop is confirmed.", scannerServiceName, endpoint.Name));
            else Assert.Fail(String.Format("Service '{0}' is still present at endpoint '{1}' after a scan stop is confirmed!", scannerServiceName, endpoint.Name));

            if (!Directory.Exists(scannerPath))
                TestContext.WriteLine(String.Format("Directory '{0}' is no longer present at endpoint '{1}' after a scan stop is confirmed.", scannerPath, endpoint.Name));
            else Assert.Fail(String.Format("Directory '{0}' is still present at endpoint '{1}' after a scan stop is confirmed!", scannerPath, endpoint.Name));

            // Checks whether a scan can be started after the stop
            StartEndpointScan(endpoint, ScanType.Full, TestContext);

            endpointStatus = endpoint.GetScanStatus();
            if (endpointStatus != ScanStatus.InProgress) Assert.Fail(String.Format("Computer '{0}' scan status is '{1}' after another scan start!", endpoint.Name, endpointStatus));
            else TestContext.WriteLine(String.Format("Computer '{0}' scan status is '{1}' after another scan start.", endpoint.Name, endpointStatus));

            if (SysHelper.IsThereService(scannerServiceName, endpoint))
                TestContext.WriteLine(String.Format("Service '{0}' is present at endpoint '{1}' after another scan start.", scannerServiceName, endpoint.Name));
            else Assert.Fail(String.Format("Service '{0}' is not present at endpoint '{1}' after another scan start!", scannerServiceName, endpoint.Name));

            // A cleanup
            StopScanning(TestContext);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Canceled, TestContext);
        }

        [TestMethod]
        public void CheckScannerAgentTest()
        { 
            Computer endpoint = DataHelper.GetEndpoint();
            string procName;

            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.InProgress, TestContext);

            if (SysHelper.IsOs64Bit(endpoint)) procName = Settings.Scanner.Default.ExeName64;
            else procName = Settings.Scanner.Default.ExeName86;

            // Checks whether the scanner process is running
            if (SysHelper.IsThereProcess(procName, endpoint))
                TestContext.WriteLine(String.Format("Process '{0}' is running at endpoint '{1}'.", procName, endpoint.Name));
            else Assert.Fail(String.Format("Process '{0}' is not running at endpoint '{1}'!", procName, endpoint.Name));

           // Checks whether there is only one scanner process
           Process[] processes = SysHelper.GetProcess(procName, endpoint);
           if (processes.Length == 1) TestContext.WriteLine(String.Format("There is only one process '{0}' at endpoint '{1}'.", procName, endpoint.Name));
           else if (processes.Length == 0) Assert.Fail(String.Format("There are no processes '{0}' at endpoint '{1}'.", procName, endpoint.Name));
           else Assert.Fail(String.Format("There is more than one process '{0}' at endpoint '{1}'!", procName, endpoint.Name));

           // Checks scanner files at the endpoint
           string scannerDir = String.Format(@"\\{0}\{1}\{2}", endpoint.Name, DataHelper.GetEndpointRoot(true), Settings.Scanner.Default.Directory);
           var files = Settings.Scanner.Default.TxtFiles;
           files.Add(procName);
           foreach (string file in files)
           {
               string filePath = String.Format(@"{0}\{1}", scannerDir, file);
               if (File.Exists(filePath)) TestContext.WriteLine(String.Format("File '{0}' exists.", filePath));
               else Assert.Fail(String.Format("File '{0}' does not exist!", filePath));
           }

           WaitForScanCompleted(ScanType.Quick);

           // Checks whether the scanner process is not running any longer
           if (!SysHelper.IsThereProcess(procName, endpoint))
               TestContext.WriteLine(String.Format("Process '{0}' is not running at endpoint '{1}' after the scan has been completed.", procName, endpoint.Name));
           else Assert.Fail(String.Format("Process '{0}' is still running at endpoint '{1}' after the scan has been completed!", procName, endpoint.Name));

           if (!Directory.Exists(scannerDir)) 
               TestContext.WriteLine(String.Format("Directory '{0}' does not exist at endpoint '{1}' after the scan has been completed.", scannerDir, endpoint.Name));
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
            StartEndpointScan(ScanType.Quick, correctLogin, invalidPassword, TestContext, endpoint);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Failed, TestContext, failedStatusWaitTime);
            ScanStatus actualScanStatus = UIHelper.GetScanStatus(endpoint);
            if (actualScanStatus != ScanStatus.Failed)
                Assert.Fail(String.Format("Scan is not '{0}' with a correct Username and invalid Password! Actual status is '{1}'!", ScanStatus.Failed, actualScanStatus));
            else TestContext.WriteLine(String.Format("Scan with a correct Username and invalid Password has '{0}' status.", actualScanStatus));

            // Scan with an invalid login and correct password
            StartEndpointScan(ScanType.Full, invalidLogin, correctPassword, TestContext, endpoint);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Failed, TestContext, failedStatusWaitTime);
            actualScanStatus = UIHelper.GetScanStatus(endpoint);
            if (actualScanStatus != ScanStatus.Failed)
                Assert.Fail(String.Format("Scan is not '{0}' with an invalid Username and correct Password! Actual status is '{1}'!", ScanStatus.Failed, actualScanStatus));
            else TestContext.WriteLine(String.Format("Scan with an invalid Username and correct Password has '{0}' status.", actualScanStatus));

            // Scan with an invalid login and invalid password
            StartEndpointScan(ScanType.Quick, invalidLogin, invalidPassword, TestContext, endpoint);
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
            SysHelper.InfectForScan(endpoint);
            SysHelper.AddUnknownFileForScan(endpoint);
            string partialNameInput = endpoint.Name.Substring(1, (int) ((endpoint.Name.Length - 1) / 2));
            string error = "error";
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickYesButton();

            InputCamCredentials(TestContext);
            WaitForValkyrieResults(TestContext);

            // Searches with a correct input
            UIMapMain.InputTextToSearchBoxParams.UISearchWatermarkTextBEditSendKeys = partialNameInput;
            UIMapMain.InputTextToSearchBox();
            TestContext.WriteLine("A part of endpoint name is entered to the searchbox at 'Valkyrie analysis results' tab.");
            UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ValkyrieAnalysis, TestContext);
            UIMapMain.ClickClearSearchTextButton();

            // Searches with an incorrect input
            UIMapResultsWindow.GoToValkyrieAnalysisResultsTab();
            TestContext.WriteLine("An incorrect search query is entered to the searchbox at 'Valkyrie analysis results' tab.");
            UIMapMain.InputTextToSearchBoxParams.UISearchWatermarkTextBEditSendKeys = partialNameInput + error;
            UIMapMain.InputTextToSearchBox();
            try
            {
                UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ValkyrieAnalysis, TestContext);
                Assert.Fail(String.Format("Endpoint '{0}' is shown at 'Valkyrie analysis results' tab after an incorrect search query!", endpoint.Name));
            }
            catch (UITestControlNotFoundException)
            {
                TestContext.WriteLine(String.Format("Endpoint '{0}' is not shown at 'Valkyrie analysis results' tab after an incorrect search query.\n", endpoint.Name));
            }
            UIMapMain.ClickClearSearchTextButton();
            
            UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ValkyrieAnalysis, TestContext); // The endpoint should appear again

            // Searches with a correct input
            UIMapResultsWindow.GoToScanResultsTab();
            UIMapMain.InputTextToSearchBoxParams.UISearchWatermarkTextBEditSendKeys = partialNameInput;
            UIMapMain.InputTextToSearchBox();
            TestContext.WriteLine("A part of endpoint name is entered to the searchbox at 'Scanning results' tab.");
            UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ScanResults, TestContext);
            UIMapMain.ClickClearSearchTextButton();

            // Searches with an incorrect input
            UIMapResultsWindow.GoToScanResultsTab();
            UIMapMain.InputTextToSearchBoxParams.UISearchWatermarkTextBEditSendKeys = partialNameInput + error;
            UIMapMain.InputTextToSearchBox();
            TestContext.WriteLine("An incorrect search query is entered to the searchbox at 'Scanning results' tab.");
            try
            {
                UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ScanResults, TestContext);
                Assert.Fail(String.Format("Endpoint '{0}' is shown on 'Scanning results' tab after an incorrect search query!", endpoint.Name));
            }
            catch (UITestControlNotFoundException)
            {
                TestContext.WriteLine(String.Format("Endpoint '{0}' is not shown on 'Scanning results' tab after an incorrect search query.\n", endpoint.Name));
            }
            UIMapMain.ClickClearSearchTextButton();

            UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ScanResults, TestContext); // The endpoint should appear again
        }

        [TestMethod]
        public void VerifyThreatDetailsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            Malware malware = SysHelper.InfectForScan(endpoint);
            SysHelper.AddUnknownFileForScan(endpoint);

            // Performs a scan
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickNoButton();
            UIHelper.WaitForControlIsClickable(StartScanButton());

            // Opens a threat details via context menu
            UIHelper.ClickRow(ResultsScreen.ScanResults, endpoint);
            UIHelper.ClickRow(ResultsScreen.ScanResults, malware);
            UIHelper.ClickRow(ResultsScreen.ScanResults, malware, MouseClick.Right);
            UIMapResultsWindow.SelectDetailsByComputersContextMenuItem();
            UIHelper.WaitForControl(CloseDetailsButton());

            // Checks all the threat details
            UIMapDetailsWindow.AssertThreatNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = malware.Name;
            UIMapDetailsWindow.AssertThreatNameTextBlockText();
            TestContext.WriteLine(String.Format("Threat name is shown correctly: '{0}'.\n", malware.Name));

            UIMapDetailsWindow.AssertFileNameValueTextBoxTextExpectedValues.UIFileNameValueTextBoxEditText = malware.Name;
            UIMapDetailsWindow.AssertFileNameValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'File Name' parameter is shown correctly: '{0}'.\n", malware.Name));

            UIMapDetailsWindow.AssertVerdictValueTextBoxTextExpectedValues.UIVerdictValueTextBoxEditText = malware.FileVerdict.ToString();
            UIMapDetailsWindow.AssertVerdictValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Verdict' parameter is shown correctly: '{0}'.\n", malware.FileVerdict));

            UIMapDetailsWindow.AssertSizeValueTextBoxTextExpectedValues.UISizeValueTextBoxEditText = malware.DisplaySize;
            UIMapDetailsWindow.AssertSizeValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Size' parameter is shown correctly: '{0}'.\n", malware.DisplaySize));

            UIMapDetailsWindow.AssertCreatedValueTextBoxTextExpectedValues.UICreatedValueTextBoxEditText = malware.Created;
            UIMapDetailsWindow.AssertCreatedValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Created' parameter is shown correctly: '{0}'.\n", malware.Created));
             
            UIMapDetailsWindow.AssertModifiedValueTextBoxTextExpectedValues.UIModifiedValueTextBoxEditText = malware.Modified;
            UIMapDetailsWindow.AssertModifiedValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Modified' parameter is shown correctly: '{0}'.\n", malware.Modified));

            UIMapDetailsWindow.AssertAccessedValueTextBoxTextExpectedValues.UIAccessedValueTextBoxEditText = malware.Accessed;
            UIMapDetailsWindow.AssertAccessedValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Accessed' parameter is shown correctly: '{0}'.\n", malware.Accessed));

            UIMapDetailsWindow.AssertHashValueTextBoxTextExpectedValues.UIHashValueTextBoxEditText = malware.Sha1;
            UIMapDetailsWindow.AssertHashValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Hash' parameter is shown correctly: '{0}'.\n", malware.Sha1));

            UIMapDetailsWindow.AssertSignerValueTextBoxTextExpectedValues.UISignerValueTextBoxEditText = malware.Signer;
            UIMapDetailsWindow.AssertSignerValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Signer' parameter is shown correctly: '{0}'.\n", malware.Signer));

            UIMapDetailsWindow.AssertDescriptionValueTextBoxTextExpectedValues.UIDescriptionValueTextEditText = malware.Description;
            UIMapDetailsWindow.AssertDescriptionValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Description' parameter is shown correctly: '{0}'.\n", malware.Description));

            UIMapDetailsWindow.AssertVersionValueTextBoxTextExpectedValues.UIVersionValueTextBoxEditText = malware.Version;
            UIMapDetailsWindow.AssertVersionValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Version' parameter is shown correctly: '{0}'.\n", malware.Version));

            UIMapDetailsWindow.AssertCopyrightValueTextBoxTextExpectedValues.UICopyrightValueTextBoEditText = malware.Copyright;
            UIMapDetailsWindow.AssertCopyrightValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Copyright' parameter is shown correctly: '{0}'.\n", malware.Copyright));

            UIMapDetailsWindow.AssertProductValueTextBoxTextExpectedValues.UIProductValueTextBoxEditText = malware.Product;
            UIMapDetailsWindow.AssertProductValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Product' parameter is shown correctly: '{0}'.\n", malware.Product));

            UIMapDetailsWindow.ClickCloseButton();
            UIHelper.WaitForControlIsClickable(StartScanButton());
        }

        [TestMethod]
        public void CheckScanResultPanelTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();            
            
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            Malware malware = SysHelper.InfectForScan(endpoint);
            TestContext.WriteLine(String.Format("Malware '{0}' is copied to the endpoint '{1}'.", malware.Name, endpoint.Name));
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickYesButton();
            AssertPanelCountersData(0, 1, TestContext);
                        
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            UnknownFile unknown = SysHelper.AddUnknownFileForScan(endpoint);
            TestContext.WriteLine(String.Format("Unknown file '{0}' is copied to the endpoint '{1}'.", unknown.Name, endpoint.Name));
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickNoButton();
            AssertPanelCountersData(1, 0, TestContext);
                        
            malware = SysHelper.InfectForScan(endpoint);
            TestContext.WriteLine(String.Format("Malware '{0}' is copied to the endpoint '{1}'.", malware.Name, endpoint.Name));
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickNoButton();
            AssertPanelCountersData(1, 1, TestContext);
            
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickYesButton();
            AssertPanelCountersData(0, 0, TestContext);
        }

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void AdQuickScanTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            string domainName = SysHelper.GetDomainName();
            Malware malware = SysHelper.InfectForScan(endpoint);
            SysHelper.AddUnknownFileForScan(endpoint);

            UIMapMain.ClickScanNowButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickActiveDirectoryButton();
            UIHelper.WaitForControlIsEnabled(NextButton());
            
            UIMapActiveDirectory.InputDomainControllerParams.UIDomainControllerTextEditText = domainName;
            UIMapActiveDirectory.InputDomainController();
            UIMapActiveDirectory.InputAdministratorParams.UIAdministratorTextBoxEditText = DataHelper.Username;
            UIMapActiveDirectory.InputAdministrator();
            UIMapActiveDirectory.InputPasswordParams.UIPasswordBoxEditSendKeys = DataHelper.EncryptedPassword;
            UIMapActiveDirectory.InputPassword();
            UIMapStartScanWizard.ClickNextButton();
            UIHelper.WaitForControl(AdTree());

            ExpandAdRow("Computers");
            CheckAdRow(endpoint.Name);
            UIMapStartScanWizard.ClickQuickScanButton();
            TestContext.WriteLine(String.Format("Quick Scan started at endpoint '{0}' from domain '{1}'...", endpoint.Name, domainName));

            WaitForScanCompleted(ScanType.Quick);
            int expectedFilesNumber = GetFoundFilesNumber(TestContext);
            UIMapScanCompletedWindow.ClickYesButton();
            InputCamCredentials(TestContext);
            WaitForValkyrieResults(TestContext);
            UIHelper.GroupByFiles();
            AssertValkyrieAnalysisResults(expectedFilesNumber, TestContext);
            UIHelper.GroupByComputers();
            UIHelper.ClickRow(ResultsScreen.ValkyrieAnalysis, endpoint);
            TestContext.WriteLine(String.Format("Computer '{0}' expanded at 'Valkyrie analysis results' screen.\n", endpoint.Name));
            UIHelper.ClickRow(ResultsScreen.ValkyrieAnalysis, malware, MouseClick.DoubleLeft);
            TestContext.WriteLine(String.Format("Malware '{0}' details opened.\n", malware.Name));
            
            UIMapValkyrieDetailsWindow.AssertThreatNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = malware.Name;
            UIMapValkyrieDetailsWindow.AssertThreatNameTextBlockText();
            UIMapValkyrieDetailsWindow.ClickCloseButton();
            UIHelper.WaitForControlIsClickable(StartScanButton());
        }

        [TestMethod]
        public void ScanningWizardNavigationNetworkAddressesTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            string newTcpPort = TextHelper.GenerateNonDefaultPort(PortType.Tcp);
            string newUdpPort = TextHelper.GenerateNonDefaultPort(PortType.Udp);
            
            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickNetworkAddressButton();
            UIHelper.WaitForControlIsEnabled(NextButton());
            
            // Inputs some non-default parameters
            UIMapNetworkAddresses.InputNetworkAddressParams.UINetworkAddressTextBoEditText = endpoint.Name;
            UIMapNetworkAddresses.InputNetworkAddress();
            UIMapNetworkAddresses.ClickAddButton();
            UIMapStartScanWizard.ClickNextButton();            
            InputScanLogin(DataHelper.Username);
            InputPassword(DataHelper.Password);
            UIMapNetworkAddresses.InputTcpPortParams.UITcpPortTextBoxEditText = newTcpPort;
            UIMapNetworkAddresses.InputTcpPort();
            UIMapNetworkAddresses.InputUdpPortParams.UIUdpPortTextBoxEditText = newUdpPort;
            UIMapNetworkAddresses.InputUdpPort();

            UIMapStartScanWizard.ClickBackButton();
            TestContext.WriteLine("Back button clicked...");
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickNetworkAddressButton();
            UIHelper.WaitForControlIsEnabled(QuickScanButton());

            // Checks whether the inputted parameters are stored after clicking Back button
            CheckStoredDataAtNetworkAddresses(endpoint, DataHelper.Username, newTcpPort, newUdpPort, TestContext);

            UIMapStartScanWizard.ClickCancelButton();
            TestContext.WriteLine("Cancel button clicked...");
            UIHelper.WaitForControlIsClickable(StartScanButton());
            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickNetworkAddressButton();
            UIHelper.WaitForControlIsEnabled(QuickScanButton());

            // Checks whether the inputted parameters are stored after clicking Cancel button
            CheckStoredDataAtNetworkAddresses(endpoint, DataHelper.Username, newTcpPort, newUdpPort, TestContext);
        }

        [TestMethod]
        public void ScanningWizardNavigationWorkgroupTest()
        {
            string workgroupName = SysHelper.GetWorkgroupName();
            string newTcpPort = TextHelper.GenerateNonDefaultPort(PortType.Tcp);
            string newUdpPort = TextHelper.GenerateNonDefaultPort(PortType.Udp);

            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControlIsEnabled(QuickScanButton());

            // Inputs some non-default parameters
            UIMapWorkgroup.InputWorkgroupParams.UIWorkgroupWatermarkTeEditSendKeys = workgroupName;
            UIMapWorkgroup.InputWorkgroup();
            UIMapWorkgroup.InputLoginParams.UILoginTextBoxEditText = DataHelper.Username;
            UIMapWorkgroup.InputLogin();
            UIMapWorkgroup.InputPasswordParams.UIPasswordBoxEditSendKeys = DataHelper.EncryptedPassword;
            UIMapWorkgroup.InputPassword();
            UIMapWorkgroup.InputTcpPortParams.UITcpPortTextBoxEditText = newTcpPort;
            UIMapWorkgroup.InputTcpPort();
            UIMapWorkgroup.InputUdpPortParams.UIUdpPortTextBoxEditText = newUdpPort;
            UIMapWorkgroup.InputUdpPort();

            UIMapStartScanWizard.ClickBackButton();
            TestContext.WriteLine("Back button clicked...");
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControlIsEnabled(QuickScanButton());

            // Checks whether the inputted parameters are stored after clicking Back button
            CheckStoredDataAtWorkgroup(workgroupName, DataHelper.Username, newTcpPort, newUdpPort, TestContext);

            UIMapStartScanWizard.ClickCancelButton();
            TestContext.WriteLine("Cancel button clicked...");
            UIHelper.WaitForControlIsClickable(StartScanButton());
            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControlIsEnabled(QuickScanButton());

            // Checks whether the inputted parameters are stored after clicking Cancel button
            CheckStoredDataAtWorkgroup(workgroupName, DataHelper.Username, newTcpPort, newUdpPort, TestContext);
        }

        [TestMethod]
        public void ScanningWizardNavigationActiveDirectoryTest()
        {
            string domainName = SysHelper.GetDomainName();
            string newTcpPort = TextHelper.GenerateNonDefaultPort(PortType.Tcp);
            string newUdpPort = TextHelper.GenerateNonDefaultPort(PortType.Udp);

            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickActiveDirectoryButton();
            UIHelper.WaitForControlIsEnabled(QuickScanButton());

            // Inputs some non-default parameters
            UIMapActiveDirectory.InputDomainControllerParams.UIDomainControllerTextEditText = domainName;
            UIMapActiveDirectory.InputDomainController();
            UIMapActiveDirectory.InputAdministratorParams.UIAdministratorTextBoxEditText = DataHelper.Username;
            UIMapActiveDirectory.InputAdministrator();
            UIMapActiveDirectory.InputPasswordParams.UIPasswordBoxEditSendKeys = DataHelper.EncryptedPassword;
            UIMapActiveDirectory.InputPassword();
            UIMapActiveDirectory.InputTcpPortParams.UIFlsTcpPortTextBoxEditText = newTcpPort;
            UIMapActiveDirectory.InputTcpPort();
            UIMapActiveDirectory.InputUdpPortParams.UIFlsUdpPortTextBoxEditText = newUdpPort;
            UIMapActiveDirectory.InputUdpPort();

            UIMapStartScanWizard.ClickBackButton();
            TestContext.WriteLine("Back button clicked...");
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickActiveDirectoryButton();
            UIHelper.WaitForControlIsEnabled(NextButton());

            // Checks whether the inputted parameters are stored after clicking Back button
            CheckStoredDataAtActiveDirectory(domainName, DataHelper.Username, newTcpPort, newUdpPort, TestContext);

            UIMapStartScanWizard.ClickCancelButton();
            TestContext.WriteLine("Cancel button clicked...");
            UIHelper.WaitForControlIsClickable(StartScanButton());
            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickActiveDirectoryButton();
            UIHelper.WaitForControlIsEnabled(NextButton());

            // Checks whether the inputted parameters are stored after clicking Cancel button
            CheckStoredDataAtActiveDirectory(domainName, DataHelper.Username, newTcpPort, newUdpPort, TestContext);
        }

        [TestMethod, Timeout(DataHelper.LongestTestTimeout)]
        public void EndpointFullScanTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            ScanType scanType = ScanType.Full;
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            Malware malware = SysHelper.InfectForScan(endpoint, scanType);
            UnknownFile unknownFile = SysHelper.AddUnknownFileForScan(endpoint, scanType);

            StartEndpointScan(endpoint, scanType, TestContext);
            WaitForScanCompleted(scanType);
            TestContext.WriteLine(String.Format("Scan '{0}' completed.", scanType));
            
            UIMapScanCompletedWindow.ClickNoButton();
            UIHelper.FindRow(malware, ResultsScreen.ScanResults, TestContext);
            UIHelper.FindRow(unknownFile, ResultsScreen.ScanResults, TestContext);
        }

        [TestMethod]
        public void InvalidInputHandlingInNetworkAddressesTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();

            UIMapMain.ClickScanNowButton();            
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickNetworkAddressButton();
            UIHelper.WaitForControlIsClickable(QuickScanButton());

            // Network Address
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyNetworkAddress, TestContext);
            UIMapNetworkAddresses.InputNetworkAddressParams.UINetworkAddressTextBoEditText = endpoint.Name;
            UIMapNetworkAddresses.InputNetworkAddress();
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyNetworkAddress, TestContext);
            UIMapNetworkAddresses.ClickAddButton();

            // Login
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyLogin, TestContext);
            UIMapNetworkAddresses.InputLoginParams.UILoginWatermarkTextBoEditSendKeys = DataHelper.Username;
            UIMapNetworkAddresses.InputLogin();

            // Password
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, TestContext);
            UIMapNetworkAddresses.InputPasswordParams.UIPasswordBoxEditSendKeys = DataHelper.EncryptedPassword;
            UIMapNetworkAddresses.InputPassword();

            // TCP port
            UIMapNetworkAddresses.InputTcpPortParams.UITcpPortTextBoxEditText = String.Empty;
            UIMapNetworkAddresses.InputTcpPort();
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyTcpPort, TestContext);
            UIMapNetworkAddresses.InputTcpPortParams.UITcpPortTextBoxEditText = TextHelper.GenerateNonDefaultPort(PortType.Tcp);
            UIMapNetworkAddresses.InputTcpPort();

            // UDP port
            UIMapNetworkAddresses.InputUdpPortParams.UIUdpPortTextBoxEditText = String.Empty;
            UIMapNetworkAddresses.InputUdpPort();
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyUdpPort, TestContext);
        }

        [TestMethod]
        public void InvalidInputHandlingInWorkgroupTest()
        {
            string workgroup = SysHelper.GetWorkgroupName();

            UIMapMain.ClickScanNowButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControlIsClickable(QuickScanButton());

            // Workgroup input
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyWorkgroup, TestContext);
            UIMapWorkgroup.InputWorkgroupParams.UIWorkgroupWatermarkTeEditSendKeys = workgroup;
            UIMapWorkgroup.InputWorkgroup();

            // Login
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyLogin, TestContext);
            UIMapWorkgroup.InputLoginParams.UILoginTextBoxEditText = DataHelper.Username;
            UIMapWorkgroup.InputLogin();

            // Password
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, TestContext);
            UIMapWorkgroup.InputPasswordParams.UIPasswordBoxEditSendKeys = DataHelper.EncryptedPassword;
            UIMapWorkgroup.InputPassword();

            // TCP port
            UIMapWorkgroup.InputTcpPortParams.UITcpPortTextBoxEditText = String.Empty;
            UIMapWorkgroup.InputTcpPort();
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyTcpPort, TestContext);
            UIMapWorkgroup.InputTcpPortParams.UITcpPortTextBoxEditText = TextHelper.GenerateNonDefaultPort(PortType.Tcp);
            UIMapWorkgroup.InputTcpPort();

            // UDP port
            UIMapWorkgroup.InputUdpPortParams.UIUdpPortTextBoxEditText = String.Empty;
            UIMapWorkgroup.InputUdpPort();
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyUdpPort, TestContext);
            UIMapWorkgroup.InputUdpPortParams.UIUdpPortTextBoxEditText = TextHelper.GenerateNonDefaultPort(PortType.Udp);
            UIMapWorkgroup.InputUdpPort();

            // Non-existent workgroup
            string error = "error";
            UIMapWorkgroup.InputWorkgroupParams.UIWorkgroupWatermarkTeEditSendKeys = workgroup.Remove(workgroup.Length - error.Length) + error;
            UIMapWorkgroup.InputWorkgroup();
            UIMapStartScanWizard.ClickQuickScanButton();
            UIHelper.WaitForControlIsClickable(ErrorTextBlock(), Settings.WaitingTime.Default.WorkgroupDiscoveryWaitTime);
            AssertErrorText(Settings.Errors.Default.WorkgroupNotFound, TestContext);
        }

        [TestMethod]
        public void InvalidInputHandlingInActiveDirectoryTest()
        {
            string domainName = SysHelper.GetDomainName();

            UIMapMain.ClickScanNowButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickActiveDirectoryButton();
            UIHelper.WaitForControlIsClickable(NextButton());

            // Domain Controller input
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyDomainController, TestContext);
            UIMapActiveDirectory.InputDomainControllerParams.UIDomainControllerTextEditText = domainName;
            UIMapActiveDirectory.InputDomainController();

            // Administrator
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyLogin, TestContext);
            UIMapActiveDirectory.InputAdministratorParams.UIAdministratorTextBoxEditText = DataHelper.Username;
            UIMapActiveDirectory.InputAdministrator();

            // Password
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, TestContext);
            UIMapActiveDirectory.InputPasswordParams.UIPasswordBoxEditSendKeys = DataHelper.EncryptedPassword;
            UIMapActiveDirectory.InputPassword();

            // TCP port
            UIMapActiveDirectory.InputTcpPortParams.UIFlsTcpPortTextBoxEditText = String.Empty;
            UIMapActiveDirectory.InputTcpPort();
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyTcpPort, TestContext);
            UIMapActiveDirectory.InputTcpPortParams.UIFlsTcpPortTextBoxEditText = TextHelper.GenerateNonDefaultPort(PortType.Tcp);
            UIMapActiveDirectory.InputTcpPort();

            // UDP port
            UIMapActiveDirectory.InputUdpPortParams.UIFlsUdpPortTextBoxEditText = String.Empty;
            UIMapActiveDirectory.InputUdpPort();
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyUdpPort, TestContext);
            UIMapActiveDirectory.InputUdpPortParams.UIFlsUdpPortTextBoxEditText = TextHelper.GenerateNonDefaultPort(PortType.Udp);
            UIMapActiveDirectory.InputUdpPort();

            // Non-existent domain
            string nonExistentDomainName = domainName + ".test";
            UIMapActiveDirectory.InputDomainControllerParams.UIDomainControllerTextEditText = nonExistentDomainName;
            UIMapActiveDirectory.InputDomainController();
            UIMapStartScanWizard.ClickNextButton();
            UIHelper.WaitForControlIsClickable(ErrorTextBlock(), Settings.WaitingTime.Default.DomainDiscoveryWaitTime);
            AssertErrorText(String.Format(Settings.Errors.Default.DomainNotFound, nonExistentDomainName), TestContext);
        }

        [TestMethod]
        public void VerifyThreatValkyrieDetailsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            UnknownFile unknownFile = SysHelper.AddUnknownFileForScan(endpoint);

            // Performs a scan
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickYesButton();
            InputCamCredentials(TestContext);
            WaitForValkyrieResults(TestContext);

            // Opens a threat details via context menu
            UIHelper.ClickRow(ResultsScreen.ValkyrieAnalysis, endpoint);
            UIHelper.ClickRow(ResultsScreen.ValkyrieAnalysis, unknownFile, MouseClick.DoubleLeft);
            UIHelper.WaitForControl(CloseValkyrieDetailsButton());

            // Checks all the threat details
            UIMapValkyrieDetailsWindow.AssertThreatNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = unknownFile.Name;
            UIMapValkyrieDetailsWindow.AssertThreatNameTextBlockText();
            TestContext.WriteLine(String.Format("Threat name is shown correctly: '{0}'.", unknownFile.Name));

            UIMapValkyrieDetailsWindow.AssertFileNameValueTextBoxTextExpectedValues.UIFileNameTextBoxEditText = unknownFile.Name;
            UIMapValkyrieDetailsWindow.AssertFileNameValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'File Name' parameter is shown correctly: '{0}'.", unknownFile.Name));

            UIMapValkyrieDetailsWindow.AssertVerdictValueTextBoxTextExpectedValues.UIVerdictTextBoxEditText = unknownFile.FileValkyrieVerdict.ToUi();
            UIMapValkyrieDetailsWindow.AssertVerdictValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Verdict' parameter is shown correctly: '{0}'.", unknownFile.FileValkyrieVerdict.ToUi()));

            UIMapValkyrieDetailsWindow.AssertSizeValueTextBoxTextExpectedValues.UISizeTextBoxEditText = unknownFile.DisplaySize;
            UIMapValkyrieDetailsWindow.AssertSizeValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Size' parameter is shown correctly: '{0}'.", unknownFile.DisplaySize));

            UIMapValkyrieDetailsWindow.AssertCreatedValueTextBoxTextExpectedValues.UICreatedTextBoxEditText = unknownFile.Created;
            UIMapValkyrieDetailsWindow.AssertCreatedValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Created' parameter is shown correctly: '{0}'.", unknownFile.Created));

            UIMapValkyrieDetailsWindow.AssertModifiedValueTextBoxTextExpectedValues.UIModifiedTextBoxEditText = unknownFile.Modified;
            UIMapValkyrieDetailsWindow.AssertModifiedValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Modified' parameter is shown correctly: '{0}'.", unknownFile.Modified));

            UIMapValkyrieDetailsWindow.AssertAccessedValueTextBoxTextExpectedValues.UIAccessedTextBoxEditText = unknownFile.Accessed;
            UIMapValkyrieDetailsWindow.AssertAccessedValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Accessed' parameter is shown correctly: '{0}'.", unknownFile.Accessed));

            UIMapValkyrieDetailsWindow.AssertTrustedVendorValueTextBoxTextExpectedValues.UITrustedVendorTextBoxEditText = unknownFile.TrustedVendor;
            UIMapValkyrieDetailsWindow.AssertTrustedVendorValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Trusted Vendor' parameter is shown correctly: '{0}'.", unknownFile.TrustedVendor));
            
            UIMapValkyrieDetailsWindow.AssertHashValueTextBoxTextExpectedValues.UIHashTextBoxEditText = unknownFile.Sha1;
            UIMapValkyrieDetailsWindow.AssertHashValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Hash' parameter is shown correctly: '{0}'.", unknownFile.Sha1));
            
            UIMapValkyrieDetailsWindow.AssertSignerValueTextBoxTextExpectedValues.UISignerTextBoxEditText = unknownFile.Signer;
            UIMapValkyrieDetailsWindow.AssertSignerValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Signer' parameter is shown correctly: '{0}'.", unknownFile.Signer));

            UIMapValkyrieDetailsWindow.AssertDescriptionValueTextBoxTextExpectedValues.UIDescriptionTextBoxEditText = unknownFile.Description;
            UIMapValkyrieDetailsWindow.AssertDescriptionValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Description' parameter is shown correctly: '{0}'.", unknownFile.Description));

            UIMapValkyrieDetailsWindow.AssertVersionValueTextBoxTextExpectedValues.UIVersionTextBoxEditText = unknownFile.Version;
            UIMapValkyrieDetailsWindow.AssertVersionValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Version' parameter is shown correctly: '{0}'.", unknownFile.Version));

            UIMapValkyrieDetailsWindow.AssertCopyrightValueTextBoxTextExpectedValues.UICopyrightTextBoxEditText = unknownFile.Copyright;
            UIMapValkyrieDetailsWindow.AssertCopyrightValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Copyright' parameter is shown correctly: '{0}'.", unknownFile.Copyright));

            UIMapValkyrieDetailsWindow.AssertProductValueTextBoxTextExpectedValues.UIProductTextBoxEditText = unknownFile.Product;
            UIMapValkyrieDetailsWindow.AssertProductValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Product' parameter is shown correctly: '{0}'.", unknownFile.Product));

            // Currently excluded from UI
            /*
            UIMapValkyrieDetailsWindow.AssertMalwareProbabilityValueTextBoxTextMatchesExpectedValues.UIMalwareProbabilityTeEditText = unknownFile.MalwareProbability; //@"\d{1,2}.\d{2}\s%";
            UIMapValkyrieDetailsWindow.AssertMalwareProbabilityValueTextBoxTextMatches();
            TestContext.WriteLine(String.Format("Threat 'Malware Probability' parameter matches an expected pattern correctly: '{0}'.", unknownFile.Product));
            */
            UIMapValkyrieDetailsWindow.ClickCloseButton();
            UIHelper.WaitForControlIsClickable(StartScanButton());
        }

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void CheckAllScanResultsTest()
        { 
            Computer endpoint = DataHelper.GetEndpoint();
            int waitTime = Settings.WaitingTime.Default.EndpointResolvingWaitTime;
            
            // Offline endpoint
            IPAddress offlineIp = SysHelper.FindOfflineIp();
            Computer offlineComputer = new Computer(offlineIp);
            
            StartEndpointScan(offlineComputer, ScanType.Quick, TestContext);
            UIHelper.WaitForControlIsEnabled(ScanResultsTab(), waitTime);
            RefreshResultsScreen();
            UIHelper.WaitForScanStatus(offlineComputer, ScanStatus.NotAccessible, TestContext);
            
            // Failed scan
            StartEndpointScan(ScanType.Quick, DataHelper.Username + "error", DataHelper.Password, TestContext, endpoint);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Failed, TestContext, waitTime);
            
            // Completed scan
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.InProgress, TestContext, waitTime);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickYesButton();
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Completed, TestContext);
        }

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void FilterDifferentScanResultsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            int waitTime = Settings.WaitingTime.Default.EndpointResolvingWaitTime;
            string error = "error";
            
            // Offline endpoint
            IPAddress offlineIp = SysHelper.FindOfflineIp();
            Computer offlineComputer = new Computer(offlineIp);
            
            StartEndpointScan(ScanType.Quick, DataHelper.Username, DataHelper.Password + error, TestContext, offlineComputer, endpoint);
            UIHelper.WaitForControlIsEnabled(StartScanButton(), waitTime);
            UIHelper.WaitForControlIsEnabled(SearchBox(), waitTime);
            RefreshResultsScreen();
            UIHelper.WaitForScanStatus(offlineComputer, ScanStatus.NotAccessible, TestContext);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Failed, TestContext);
                        
            SetStatusFilter(ScanStatus.NotAccessible, TestContext);
            UIHelper.WaitForResultsRow(offlineComputer, ResultsScreen.ScanResults, TestContext);
            UIHelper.WaitForAbsentResultsRow(endpoint, ResultsScreen.ScanResults, TestContext);
            UIMapResultsWindow.ClickClearFilterButton();
        
            // Failed scan
            UIHelper.ClickRow(ResultsScreen.ScanResults, endpoint);
            SetStatusFilter(ScanStatus.Failed, TestContext);
            UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ScanResults, TestContext);
            UIHelper.WaitForAbsentResultsRow(offlineComputer, ResultsScreen.ScanResults, TestContext);
            UIMapResultsWindow.ClickClearFilterButton();
            
            // In Progress scan
            StartEndpointScan(ScanType.Quick, TestContext, endpoint, offlineComputer);
            UIHelper.WaitForControlIsEnabled(StopScanButton(), waitTime);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.InProgress, TestContext);
            UIHelper.WaitForScanStatus(offlineComputer, ScanStatus.NotAccessible, TestContext);
            
            SetStatusFilter(ScanStatus.InProgress, TestContext);
            UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ScanResults, TestContext);
            UIHelper.WaitForAbsentResultsRow(offlineComputer, ResultsScreen.ScanResults, TestContext);
            UIMapResultsWindow.ClickClearFilterButton();
            UIHelper.ClickRow(ResultsScreen.ScanResults, endpoint);
            
            // Completed scan
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickYesButton();            
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Completed, TestContext);            
            
            SetStatusFilter(ScanStatus.Completed, TestContext);
            UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ScanResults, TestContext);
            UIHelper.WaitForAbsentResultsRow(offlineComputer, ResultsScreen.ScanResults, TestContext);
            UIMapResultsWindow.ClickClearFilterButton();
            
            // Canceled scan
            StartEndpointScan(ScanType.Full, TestContext, endpoint, offlineComputer);
            UIHelper.WaitForControlIsEnabled(StopScanButton(), waitTime);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.InProgress, TestContext, waitTime);
            UIHelper.WaitForScanStatus(offlineComputer, ScanStatus.NotAccessible, TestContext);
            StopScanning(TestContext);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Canceled, TestContext, waitTime);

            SetStatusFilter(ScanStatus.Canceled, TestContext);
            UIHelper.WaitForResultsRow(endpoint, ResultsScreen.ScanResults, TestContext);
            UIHelper.WaitForAbsentResultsRow(offlineComputer, ResultsScreen.ScanResults, TestContext);
            UIMapResultsWindow.ClickClearFilterButton();
        }

        [TestMethod]
        public void QuickScanOfIpRangeTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            Computer server = new Computer();
            IPAddress internalAddress = SysHelper.GetLocalIpAddress(true);
            string ipRange = String.Format("{0}-{1}", internalAddress.ChangeLastByte(0), internalAddress.ChangeLastByte(255));
            UnknownFile unknownFile = SysHelper.AddUnknownFileForScan(endpoint);
            Malware malware = SysHelper.InfectForScan(endpoint);

            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickNetworkAddressButton();

            UIHelper.WaitForControlIsEnabled(QuickScanButton());
            CleanNetworkAddressesList();
            UIMapNetworkAddresses.InputNetworkAddressParams.UINetworkAddressTextBoEditText = ipRange;
            UIMapNetworkAddresses.InputNetworkAddress();
            UIMapNetworkAddresses.ClickAddButton();
            TestContext.WriteLine(String.Format("IP range {0} added to the addresses list.", ipRange));

            UIMapNetworkAddresses.InputLoginParams.UILoginWatermarkTextBoEditSendKeys = DataHelper.Username;
            UIMapNetworkAddresses.InputLogin();
            UIMapNetworkAddresses.InputPasswordParams.UIPasswordBoxEditSendKeys = DataHelper.EncryptedPassword;
            UIMapNetworkAddresses.InputPassword();

            UIMapStartScanWizard.ClickQuickScanButton();

            UIHelper.WaitForControl(ScanCompletedTextBlock(), Settings.WaitingTime.Default.QuickScanRangeWaitTime);
            UIMapScanCompletedWindow.ClickNoButton();

            // Verifies the scanning results
            UIHelper.ClickRow(ResultsScreen.ScanResults, server);
            TestContext.WriteLine(String.Format("Computer '{0}' found at 'Scanning results' screen.", server.Name));
            UIHelper.ClickRow(ResultsScreen.ScanResults, endpoint);
            TestContext.WriteLine(String.Format("Computer '{0}' found at 'Scanning results' screen.", endpoint.Name));
            UIHelper.ClickRow(ResultsScreen.ScanResults, malware);
            TestContext.WriteLine(String.Format("Malware '{0}' found at 'Scanning results' screen.", malware.Location));
            UIHelper.ClickRow(ResultsScreen.ScanResults, unknownFile);
            TestContext.WriteLine(String.Format("Unknown file '{0}' found at 'Scanning results' screen.", unknownFile.Location));
        }

        [TestMethod]
        public void QuickScanOfSubnetTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            Computer server = new Computer();
            IPAddress internalAddress = SysHelper.GetLocalIpAddress(true);
            string subnet = String.Format("{0}/{1}", internalAddress.ChangeLastByte(0), 24);
            UnknownFile unknownFile = SysHelper.AddUnknownFileForScan(endpoint);
            Malware malware = SysHelper.InfectForScan(endpoint);

            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickNetworkAddressButton();

            UIHelper.WaitForControlIsEnabled(QuickScanButton());
            CleanNetworkAddressesList();
            UIMapNetworkAddresses.InputNetworkAddressParams.UINetworkAddressTextBoEditText = subnet;
            UIMapNetworkAddresses.InputNetworkAddress();
            UIMapNetworkAddresses.ClickAddButton();
            TestContext.WriteLine(String.Format("Subnet {0} added to the addresses list.", subnet));

            UIMapNetworkAddresses.InputLoginParams.UILoginWatermarkTextBoEditSendKeys = DataHelper.Username;
            UIMapNetworkAddresses.InputLogin();
            UIMapNetworkAddresses.InputPasswordParams.UIPasswordBoxEditSendKeys = DataHelper.EncryptedPassword;
            UIMapNetworkAddresses.InputPassword();

            UIMapStartScanWizard.ClickQuickScanButton();

            UIHelper.WaitForControl(ScanCompletedTextBlock(), Settings.WaitingTime.Default.QuickScanRangeWaitTime);
            UIMapScanCompletedWindow.ClickNoButton();

            // Verifies the scanning results
            UIHelper.ClickRow(ResultsScreen.ScanResults, server);
            TestContext.WriteLine(String.Format("Computer '{0}' found at 'Scanning results' screen.", server.Name));
            UIHelper.ClickRow(ResultsScreen.ScanResults, endpoint);
            TestContext.WriteLine(String.Format("Computer '{0}' found at 'Scanning results' screen.", endpoint.Name));
            UIHelper.ClickRow(ResultsScreen.ScanResults, malware);
            TestContext.WriteLine(String.Format("Malware '{0}' found at 'Scanning results' screen.", malware.Location));
            UIHelper.ClickRow(ResultsScreen.ScanResults, unknownFile);
            TestContext.WriteLine(String.Format("Unknown file '{0}' found at 'Scanning results' screen.", unknownFile.Location));
        }

        [TestMethod]
        public void CheckValkyrieAnalysisResultsPanelTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            UnknownFile unknown = SysHelper.AddUnknownFileForScan(endpoint);                        
            TestContext.WriteLine(String.Format("Unknown file '{0}' is copied to the endpoint '{1}'.", unknown.Name, endpoint.Name));
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickYesButton();
            InputCamCredentials(TestContext);
            WaitForValkyrieResults(TestContext);            
            UIHelper.ClickRow(ResultsScreen.ValkyrieAnalysis, endpoint);
            UIHelper.WaitForCellContent(ResultsScreen.ValkyrieAnalysis, unknown, Settings.ResultsValkyrie.Default.VerdictCellNumber, ValkyrieVerdict.NoThreatFound.ToUi(), 
                                                                                                                TestContext, Settings.WaitingTime.Default.NoThreatFoundVerdictWaitTime);
            AssertPanelCountersData(1, 0, TestContext);

            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            unknown = SysHelper.AddUnknownFileForScan(endpoint);
            TestContext.WriteLine(String.Format("Unknown file '{0}' is copied to the endpoint '{1}'.", unknown.Name, endpoint.Name));
            Malware malware = SysHelper.InfectForScan(endpoint);
            TestContext.WriteLine(String.Format("Malware '{0}' is copied to the endpoint '{1}'.", malware.Name, endpoint.Name));
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickYesButton();
            InputCamCredentials(TestContext);
            WaitForValkyrieResults(TestContext);
            UIHelper.ClickRow(ResultsScreen.ValkyrieAnalysis, endpoint);
            UIHelper.WaitForCellContent(ResultsScreen.ValkyrieAnalysis, unknown, Settings.ResultsValkyrie.Default.VerdictCellNumber, ValkyrieVerdict.NoThreatFound.ToUi(),
                                                                                                                TestContext, Settings.WaitingTime.Default.NoThreatFoundVerdictWaitTime);
            AssertPanelCountersData(1, 1, TestContext);
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