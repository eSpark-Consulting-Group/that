using System;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestFAT.UIMaps.StartScanWizard.UIMapWorkgroupClasses;
using CodedUITestFAT.UIMaps.StartScanWizard.UIMapActiveDirectoryClasses;


namespace CodedUITestFAT
{
    /// <summary> Tests for the Main screen. </summary>
    [CodedUITest]
    public sealed class CodedUITestMain : CodedUITestCommon
    {
        public CodedUITestMain()
        {
        }

        #region UIMap properties
        
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

        #endregion

        #region Initialize and cleanup

        [TestInitialize]
        public void ATTestInitialize()
        {
            SysHelper.StartProduct(DataHelper.GetEndpoint());
            
            CodedUITestEula useEula = new CodedUITestEula();            
            useEula.UIMapEula.ClickIAcceptButton();            
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

        #region Discovery

        private WpfButton ScanNowButton()
        {
            return UIMapDiscovery.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIPleaseWaitProgressBar.UIScanNowButtonButton;
        }

        private WpfButton CustomScanButton()
        {
            return UIMapDiscovery.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UICustomScanButtonButton;
        }

        private WpfButton StartAutoDiscoveryButton()
        {
            return UIMapDiscovery.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIPleaseWaitProgressBar.UIStartAutoDiscoveryBuButton;
        }

        #endregion

        #region Main screen

        internal WpfWindow MainWindow()
        {
            return UIMapMain.UIForensicAnalysisToolWindow;
        }
        
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

        private WpfEdit SearchBox()
        {
          return UIMapMain.UIUnknownFileHunterWindow1.UISearchControlCustom.UISearchWatermarkTextBEdit;
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

        #region Start Scan Wizard                

        #region Active Directory

        private WpfTree AdTree()
        {
           return UIMapActiveDirectory.UIForensicAnalysisToolWindow.UIActiveDirectoryViewCustom.UIAdTreeTree;
        }

        private WpfEdit DomainNameTextBox()
        {
            return UIMapActiveDirectory.UIUnknownFileHunterWindow1.UIThisCustom.UIDomainNameWatermarkTEdit;
        }

        #endregion

        #region Network Addresses
        
        private WpfText ErrorTextBlock()
        {
            return UIMapStartScanWizard.UIUnknownFileHunterWindow.UIScanningWizardWindowCustom.UIPleaseSpecifyPassworText;
        }

        #endregion

        #region Workgroup

        private WpfTreeItem WorkgroupTreeItem(string workgroupName)
        {
            UIMapWorkgroup.UIForensicAnalysisToolWindow.UIWorkgroupViewCustom.UIWorkgroupTreeTree.UIWorkgroupTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name]
                        = workgroupName;
            return UIMapWorkgroup.UIForensicAnalysisToolWindow.UIWorkgroupViewCustom.UIWorkgroupTreeTree.UIWorkgroupTreeItem;
        }

        #endregion

        #endregion
        
        #region Results window

        internal WpfTabPage ScanResultsTab()
        {     
            return UIMapResultsWindow.UIUnknownFileHunterWindow.UIResultsScreenCustom.UIResultsTabControlTabList.UIUnknownfilesTabPage;
        }

        internal WpfTabPage ValkyrieAnalysisResultsTab()
        {
            return UIMapResultsWindow.UIUnknownFileHunterWindow1.UIResultsScreenCustom.UIResultsTabControlTabList.UIValkyrieanalysisresuTabPage;
        }
        
        internal WpfTable ScanResultsByComputersTable()
        {            
            return UIMapResultsWindow.UIForensicAnalysisToolWindow.UIScanResultsByComputeCustom.UIRadTreeListViewTable;
        }

        internal WpfTable ScanResultsByFilesTable()
        {
            return UIMapResultsWindow.UIForensicAnalysisToolWindow.UIScanResultsByFilesScCustom.UIRadTreeListViewTable;
        }

        private WpfText SendingFilesTextBlock()
        {
            return UIMapResultsWindow.UIUnknownFileHunterWindow1.UIViewsMainViewxamlPane.UISendingFilesforAnalyProgressBar.UISendingFilesforAnalyText;
        }
        private WpfText ResolvingComputersTextBlock()
        {
            return UIMapResultsWindow.UIUnknownFileHunterWindow1.UIViewsMainViewxamlPane.UIResolvingComputersProgressBar.UIResolvingComputersText;
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

        /// <summary> Asserts the counters values for the bottom panel. </summary>
        /// <param name="expectedUnknownCount">Set an expected Unknown files counter value.</param>
        /// <param name="expectedMalwareCount">Set an expected Malicious file counter value.</param>
        /// <param name="expectedInAnalysisCount">Set an expected In Analysis files counter value.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        internal void AssertPanelCountersData(int expectedUnknownCount, int expectedMalwareCount, int expectedInAnalysisCount, TestContext testContext)
        {
            UIMapMain.AssertUnknownFilesCounterTextBlockTextExpectedValues.UIItem0TextDisplayText = expectedUnknownCount.ToString();
            UIMapMain.AssertUnknownFilesCounterTextBlockText();
            UIMapMain.AssertMalwaresCounterTextBlockTextExpectedValues.UIItem0Text1DisplayText = expectedMalwareCount.ToString();
            UIMapMain.AssertMalwaresCounterTextBlockText();

            if (expectedInAnalysisCount == 0)
            {
                testContext.WriteLine(String.Format("The bottom panel shows '{0}' unknown and '{1}' malicious files.", expectedUnknownCount, expectedMalwareCount));
            }
            else
            {
                UIMapMain.AssertInAnalysisCounterTextBlockTextExpectedValues.UIItem1TextDisplayText = expectedInAnalysisCount.ToString();
                UIMapMain.AssertInAnalysisCounterTextBlockText();
                testContext.WriteLine(String.Format("The bottom panel shows '{0}' Unknown, '{1}' Malicious and '{2}' In Analysis files.", expectedUnknownCount, expectedMalwareCount, expectedInAnalysisCount));
            }
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

        /// <summary> Waits until the Valkyrie results are received. </summary>
        /// <param name="testContext">Set a current TestContext.</param>
        internal void WaitForValkyrieResults(TestContext testContext)
        {
            int waitTime = Settings.WaitingTime.Default.ValkyrieAnalysisWaitTime;
            UIHelper.WaitForControl(ValkyrieAnalysisResultsTab(), waitTime);
            UIHelper.WaitForControlIsEnabled(ValkyrieAnalysisResultsTab(), waitTime);
            testContext.WriteLine("'Valkyrie analysis results' tab is enabled...");

            // Waits until the Valkyrie upload progress is completed
            UIHelper.WaitForControlIsClickable(SendingFilesTextBlock(), waitTime, false);
            var timer = Stopwatch.StartNew();
            for (; ; )
            {
                Playback.Wait(2000);
                if (!MainWindowTitleTextBlock().DisplayText.Contains("%"))
                {
                    testContext.WriteLine("Valkyrie upload progress is completed...");
                    break;
                }
                if ((int) timer.ElapsedMilliseconds > waitTime) throw new Exception(String.Format("Valkyrie upload progress is not completed after {0} minutes!", waitTime / 60000));
            }
            timer.Stop();

            // CodedUI for some reasons does not detect a tab content without switching/refreshing the tabs
            UIHelper.GroupByFiles();
            Playback.Wait(1000);
            UIHelper.GroupByComputers();
            
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
               
        /// <summary> Expands a given row in Active Directory tree. In accordance with DeploymentItem.cs. </summary>
        /// <param name="rowName">Set a unique row name.</param>
        private void ExpandAdRow(string rowName)
        {
            //UIMapActiveDirectory.UIUnknownFileHunterWindow.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDCdevTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name] = SysHelper.GetDomainName(true);
            //UIMapActiveDirectory.UIUnknownFileHunterWindow.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDCdevTreeItem.UIComputersTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name] = rowName;
            UIMapActiveDirectory.UIForensicAnalysisToolWindow.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDevTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name] = SysHelper.GetDomainName(true);
            UIMapActiveDirectory.UIForensicAnalysisToolWindow.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDevTreeItem.UIComputersTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name] = rowName;

            UIMapActiveDirectory.ExpandAdRow();            
        }

        /// <summary> Checks a given row in Active Directory tree. In accordance with DeploymentItem.cs. </summary>
        /// <param name="rowName">Set a unique row name.</param>
        private void CheckAdRow(string rowName)
        {
            UIMapActiveDirectory.UIForensicAnalysisToolWindow.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDevTreeItem.
                                                                                            SearchProperties[WpfTreeItem.PropertyNames.Name] = SysHelper.GetDomainName(true);
            UIMapActiveDirectory.UIForensicAnalysisToolWindow.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDevTreeItem.UIComputersTreeItem.UIALEXCHEWIN7PROTreeItem.
                                                                                            SearchProperties[WpfTreeItem.PropertyNames.Name] = rowName;           
            UIMapActiveDirectory.CheckAdRow();
        }

        private bool IsDomainComputerChecked(Computer endpoint)
        {
            WpfTreeItem domainTreeItem = UIMapActiveDirectory.UIUnknownFileHunterWindow1.UIScanningWizardWindowCustom.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDevTreeItem;
            domainTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name] = SysHelper.GetDomainName(true);

            WpfTreeItem computersTreeItem = domainTreeItem.GetChildren().FirstOrDefault(i => i is WpfTreeItem && i.Name == Settings.General.Default.AdTreeComputersElement) as WpfTreeItem;
            if (computersTreeItem == null) throw new Exception(String.Format("Tree item '{0}' not found!", Settings.General.Default.AdTreeComputersElement));
            WpfTreeItem endpointTreeItem = computersTreeItem.GetChildren().FirstOrDefault(i => i is WpfTreeItem && i.Name == endpoint.Name.ToUpper()) as WpfTreeItem;
            if (endpointTreeItem == null) throw new Exception(String.Format("Tree item '{0}' not found!", endpoint.Name.ToUpper()));
            WpfCheckBox endpointCheckBox = endpointTreeItem.GetChildren().FirstOrDefault(i => i is WpfCheckBox) as WpfCheckBox;

            return endpointCheckBox.Checked;
        }

        /// <summary> Waits for a domain computer item in AD tree. </summary>
        /// <param name="endpoint">Set a computer name.</param>
        private void WaitForDomainComputer(Computer endpoint)
        {
            int waitTime = Settings.WaitingTime.Default.DomainTreeItemExpansionWaitTime;

            var timer = Stopwatch.StartNew();
            for (;;)
            {
                Playback.Wait(2000);
                try
                {
                    IsDomainComputerChecked(endpoint);
                    break;
                } catch (Exception){ }
                if ((int) timer.ElapsedMilliseconds > waitTime)
                            throw new Exception(String.Format("Domain computer '{0}' is not available at AD tree after {1} seconds!", endpoint.Name, waitTime / 1000));
            }
            timer.Stop();
        }

        /// <summary> Checks a given workgroup in the workgroups tree. </summary>
        /// <param name="workgroupName">Set a workgroup name.</param>
        private void CheckWorkgroup(string workgroupName)
        {
            UIMapWorkgroup.UIForensicAnalysisToolWindow.UIWorkgroupViewCustom.UIWorkgroupTreeTree.UIWorkgroupTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name] 
                        = workgroupName;
            UIMapWorkgroup.CheckWorkgroup();
        }

        /// <summary> Determines whether a given workgroup tree item is checked.  </summary>
        /// <param name="workgroupName">Set a workgroup name.</param>
        private bool IsWorkgroupChecked(string workgroupName)
        {
            WpfTreeItem workgroupItem = WorkgroupTreeItem(workgroupName);
            WpfCheckBox workgroupCheckBox = workgroupItem.GetChildren().FirstOrDefault(e => e is WpfCheckBox) as WpfCheckBox; // Gets the workgroup tree item checkbox
            return workgroupCheckBox.Checked;
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
        
        /// <summary> Inputs Domain Name, if necessary. </summary>
        /// <param name="domainName">Set a domain name to input.</param>
        private void InputDomainName(string domainName)
        {
            WpfEdit domainNameEdit = UIMapActiveDirectory.UIForensicAnalysisToolWindow.UIThisCustom.UIDomainNameWatermarkTEdit;
            if (domainNameEdit.Text == Settings.General.Default.EmptyWatermarkTextBoxText || domainNameEdit.Text != domainName) domainNameEdit.Text = domainName;
        }

        /// <summary> Inputs Domain Administrator, if necessary. </summary>
        /// <param name="domainAdministrator">Set a domain administrator to input.</param>
        private void InputDomainAdministrator(string domainAdministrator)
        {
            WpfEdit domainAdministratorEdit = UIMapActiveDirectory.UIForensicAnalysisToolWindow.UIThisCustom.UIDomainAdministratorWEdit;
            if (domainAdministratorEdit.Text == Settings.General.Default.EmptyWatermarkTextBoxText || domainAdministratorEdit.Text != domainAdministrator) domainAdministratorEdit.Text = domainAdministrator;
        }

        /// <summary> Inputs Workgroup, if necessary. </summary>
        /// <param name="workgroupName">Set a workgroup name to input.</param>
        private void InputWorkgroup(string workgroupName)
        {
            WpfEdit workgroupEdit = UIMapWorkgroup.UIUnknownFileHunterWindow1.UIWorkgroupViewCustom.UIWorkgroupWatermarkTeEdit;
            if (workgroupEdit.Text == Settings.General.Default.EmptyWatermarkTextBoxText || workgroupEdit.Text != workgroupName) workgroupEdit.Text = workgroupName;
        }

        #endregion

        #region Test methods        

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void WorkgroupQuickScanAndThreatDetailsCheckTest()
        {

            UIHelper.WaitForControlIsEnabled(SkipDiscoveryButton());
            UIMapDiscovery.ClickSkipDiscoveryButton();

            UIHelper.WaitForControlIsEnabled(CustomScanButton());

            Computer endpoint = DataHelper.GetEndpoint();
            string workgroupName = SysHelper.GetWorkgroupName();

            Malware malware = SysHelper.InfectForScan(endpoint);
            UnknownFile unknownFile = SysHelper.AddUnknownFileForScan(endpoint);

            UIHelper.WaitForControlIsClickable(CustomScanButton());
            UIMapDiscovery.ClickCustomScanButton();

            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControlIsEnabled(NextButton());
            UIMapWorkgroup.SelectUseKnownWorkgroupRadioButton();            
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

            UIHelper.GroupByFiles();
            UIHelper.ClickRow(malware);
            TestContext.WriteLine(String.Format("Malicious file {0} found at the results screen.", malware));
            UIHelper.ClickRow(unknownFile);
            TestContext.WriteLine(String.Format("Unknown file {0} found at the results screen.", unknownFile));

            UIHelper.ClickRow(malware, MouseClick.DoubleLeft);
            UIMapDetailsWindow.AssertThreatNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = malware.Name;
            UIMapDetailsWindow.AssertThreatNameTextBlockText();
            TestContext.WriteLine(String.Format("Malicious file name {0} found at threat Details screen.", malware.Name));
            UIMapDetailsWindow.ClickCloseButton();

            UIHelper.ClickRow(unknownFile, MouseClick.DoubleLeft);
            UIMapDetailsWindow.AssertThreatNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = unknownFile.Name;
            UIMapDetailsWindow.AssertThreatNameTextBlockText();
            TestContext.WriteLine(String.Format("Unknown file name {0} found at threat Details screen.", unknownFile.Name));
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

            if (SkipDiscoveryButton().Enabled) TestContext.WriteLine("Skip Discovery button is enabled by default.");
            else Assert.Fail("Skip Discovery button is enabled by default!");

            if (!PreviousScansButton().Enabled) TestContext.WriteLine("Previous Scans button is disabled by default.");
            else Assert.Fail("Previous Scans button is enabled by default!");           

            if (!CreateReportDropDownButton().Enabled) TestContext.WriteLine("Create Report drop down button is disabled by default.");
            else Assert.Fail("Stop Scan button is enabled by default!");

            if (!OptionsButton().Enabled) TestContext.WriteLine("Options button is disabled by default.");
            else Assert.Fail("Options button is enabled by default!");

            if (useHelp.HelpButton().Enabled) TestContext.WriteLine("Help button is enabled by default.");
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
        
        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void AdQuickScanTest()
        {
            UIHelper.WaitForControlIsClickable(SkipDiscoveryButton());
            UIMapDiscovery.ClickSkipDiscoveryButton();

            Computer endpoint = DataHelper.GetEndpoint();
            string domainName = SysHelper.GetDomainName();
            Malware malware = SysHelper.InfectForScan(endpoint);
            UnknownFile unknownFile = SysHelper.AddUnknownFileForScan(endpoint);                     

            UIHelper.WaitForControlIsClickable(CustomScanButton());
            UIMapDiscovery.ClickCustomScanButton();

            UIHelper.WaitForControlIsClickable(NetworkAddressButton());

            UIMapStartScanWizard.ClickActiveDirectoryButton();
            UIHelper.WaitForControlIsEnabled(NextButton());            
                                    
            InputDomainName(domainName);
            InputDomainAdministrator(DataHelper.Username);
            InputPassword(DataHelper.Password);

            UIMapStartScanWizard.ClickNextButton();

            UIHelper.WaitForControl(AdTree());

            ExpandAdRow(Settings.General.Default.AdTreeComputersElement);
            CheckAdRow(endpoint.Name);
            UIMapStartScanWizard.ClickNextButton();

            UIHelper.WaitForControl(QuickScanButton());
            UIMapStartScanWizard.ClickQuickScanButton();

            TestContext.WriteLine(String.Format("Quick Scan started at endpoint '{0}' from domain '{1}'...", endpoint.Name, domainName));

            WaitForScanCompleted(ScanType.Quick);

            UIHelper.GroupByFiles();
            UIHelper.ClickRow(malware);
            TestContext.WriteLine(String.Format("Malicious file {0} found at the results screen.", malware));
            UIHelper.ClickRow(unknownFile);
            TestContext.WriteLine(String.Format("Unknown file {0} found at the results screen.", unknownFile));

            UIHelper.ClickRow(malware, MouseClick.DoubleLeft);
            UIMapDetailsWindow.AssertThreatNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = malware.Name;
            UIMapDetailsWindow.AssertThreatNameTextBlockText();
            TestContext.WriteLine(String.Format("Malicious file name {0} found at threat Details screen.", malware.Name));
            UIMapDetailsWindow.ClickCloseButton();

            UIHelper.ClickRow(unknownFile, MouseClick.DoubleLeft);
            UIMapDetailsWindow.AssertThreatNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = unknownFile.Name;
            UIMapDetailsWindow.AssertThreatNameTextBlockText();
            TestContext.WriteLine(String.Format("Unknown file name {0} found at threat Details screen.", unknownFile.Name));
            UIMapDetailsWindow.ClickCloseButton();
        }
        
        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void ScanningWizardNavigationWorkgroupTest()
        {
            string workgroupName = SysHelper.GetWorkgroupName();
            
            SysHelper.DisinfectForQuickScan(DataHelper.GetEndpoint(), TestContext);

            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControlIsEnabled(NextButton());

            // Specify Workgroup step
            UIMapWorkgroup.SelectUseKnownWorkgroupRadioButton();
            UIMapWorkgroup.InputWorkgroupParams.UIWorkgroupWatermarkTeEditSendKeys = workgroupName;
            UIMapWorkgroup.InputWorkgroup();
            UIMapStartScanWizard.ClickBackButton();
            TestContext.WriteLine("Back to Start Scan Wizard...");
            UIHelper.WaitForControl(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControl(NextButton());
            TestContext.WriteLine("Returned to Specify Workgroup step...");
            UIMapWorkgroup.AssertWorkgroupTextBoxTextExpectedValues.UIWorkgroupWatermarkTeEditText = workgroupName;
            UIMapWorkgroup.AssertWorkgroupTextBoxText();
            TestContext.WriteLine(String.Format("Workgroup name '{0}' is preserved at Specify Workgroup step.", workgroupName));
            UIMapStartScanWizard.ClickNextButton();

            // Select Computers step
            UIHelper.WaitForControl(WorkgroupTreeItem(workgroupName), Settings.WaitingTime.Default.WorkgroupDiscoveryWaitTime);
            UIHelper.WaitForControlIsEnabled(WorkgroupTreeItem(workgroupName));
            CheckWorkgroup(workgroupName);
            UIMapStartScanWizard.ClickBackButton();
            TestContext.WriteLine("Back to Specify Workgroup step...");
            UIMapStartScanWizard.ClickNextButton();
            TestContext.WriteLine("Returned to Select Computers step...");
            UIHelper.WaitForControl(WorkgroupTreeItem(workgroupName));
            if (IsWorkgroupChecked(workgroupName)) TestContext.WriteLine(String.Format("Workgroup '{0}' is still selected at Select Computers step.", workgroupName));
            else Assert.Fail(String.Format("Workgroup '{0}' is not selected at Select Computers step!", workgroupName));
            UIMapStartScanWizard.ClickNextButton();

            // Scan Credentials step
            UIHelper.WaitForControlIsEnabled(QuickScanButton());
            InputScanLogin(DataHelper.Username);
            InputPassword(DataHelper.Password);
            UIMapStartScanWizard.ClickBackButton();
            TestContext.WriteLine("Back to Select Computers step...");
            UIHelper.WaitForControl(NextButton());
            UIMapStartScanWizard.ClickNextButton();
            TestContext.WriteLine("Returned to Scan Credentials step...");
            UIMapWorkgroup.AssertLoginTextBoxTextExpectedValues.UILoginWatermarkTextBoEditText = DataHelper.Username;
            UIMapWorkgroup.AssertLoginTextBoxText();
            TestContext.WriteLine(String.Format("Login '{0}' is preserved.", DataHelper.Username));
            UIMapStartScanWizard.ClickQuickScanButton();
            TestContext.WriteLine("Quick Scan button clicked...");

            WaitForScanCompleted(ScanType.Quick);
            //UIMapScanCompletedWindow.ClickCloseCrossButton();
            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControl(NextButton());

            // Specify Workgroup step after scan
            TestContext.WriteLine("Returned to Specify Workgroup step after scan...");
            UIMapWorkgroup.AssertWorkgroupTextBoxTextExpectedValues.UIWorkgroupWatermarkTeEditText = workgroupName;
            UIMapWorkgroup.AssertWorkgroupTextBoxText();
            TestContext.WriteLine(String.Format("Workgroup name '{0}' is preserved at Specify Workgroup step.", workgroupName));
            UIMapStartScanWizard.ClickNextButton();

            // Select Computers step after scan
            TestContext.WriteLine("Returned to Select Computers step after scan...");
            UIHelper.WaitForControl(WorkgroupTreeItem(workgroupName), Settings.WaitingTime.Default.WorkgroupDiscoveryWaitTime);
            UIHelper.WaitForControlIsEnabled(WorkgroupTreeItem(workgroupName));
            if (!IsWorkgroupChecked(workgroupName)) TestContext.WriteLine(String.Format("Workgroup '{0}' is not selected at Select Computers step.", workgroupName));
            else Assert.Fail(String.Format("Workgroup '{0}' is still selected at Select Computers step!", workgroupName));
            CheckWorkgroup(workgroupName);
            UIMapStartScanWizard.ClickNextButton();

            // Scan Credentials step after scan
            UIHelper.WaitForControlIsEnabled(QuickScanButton());
            TestContext.WriteLine("Returned to Scan Credentials step after scan...");
            UIMapWorkgroup.AssertLoginTextBoxTextExpectedValues.UILoginWatermarkTextBoEditText = DataHelper.Username;
            UIMapWorkgroup.AssertLoginTextBoxText();
            TestContext.WriteLine(String.Format("Login '{0}' is preserved.", DataHelper.Username));
            UIMapStartScanWizard.ClickQuickScanButton();
            TestContext.WriteLine("Quick Scan button clicked...");
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, TestContext);
        }

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void ScanningWizardNavigationActiveDirectoryTest()
        {
            string domainName = SysHelper.GetDomainName();
            Computer endpoint = DataHelper.GetEndpoint();
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);

            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickActiveDirectoryButton();
            UIHelper.WaitForControlIsEnabled(NextButton());

            // Active Directory Credentials step
            InputDomainName(domainName);
            InputDomainAdministrator(DataHelper.Username);
            InputPassword(DataHelper.Password);
            UIMapStartScanWizard.ClickBackButton();
            TestContext.WriteLine("Back to Start Scan Wizard...");
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickActiveDirectoryButton();
            TestContext.WriteLine("Returned to Active Directory Credentials step...");
            UIHelper.WaitForControlIsEnabled(NextButton());
            UIMapActiveDirectory.AssertDomainNameTextBoxTextExpectedValues.UIDomainNameWatermarkTEditText = domainName;
            UIMapActiveDirectory.AssertDomainNameTextBoxText();
            TestContext.WriteLine(String.Format("Domain Name '{0}' preserved.", domainName));
            UIMapActiveDirectory.AssertDomainAdministratorTextBoxTextExpectedValues.UIDomainAdministratorWEditText = DataHelper.Username;
            UIMapActiveDirectory.AssertDomainAdministratorTextBoxText();
            TestContext.WriteLine(String.Format("Domain Administrator '{0}' preserved.", DataHelper.Username));
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, TestContext);
            InputPassword(DataHelper.Password);
            UIMapStartScanWizard.ClickNextButton();

            // Select Computers step
            UIHelper.WaitForControl(AdTree());
            ExpandAdRow(Settings.General.Default.AdTreeComputersElement);
            WaitForDomainComputer(endpoint);
            CheckAdRow(endpoint.Name);
            UIMapStartScanWizard.ClickBackButton();
            TestContext.WriteLine("Back to Active Directory Credentials step...");
            UIHelper.WaitForControl(DomainNameTextBox());
            UIMapStartScanWizard.ClickNextButton();
            TestContext.WriteLine("Returned to Select Computers step...");
            UIHelper.WaitForControl(AdTree());
            WaitForDomainComputer(endpoint);
            if (IsDomainComputerChecked(endpoint)) TestContext.WriteLine(String.Format("Endpoint '{0}' remains checked in AD tree.", endpoint.Name));
            else Assert.Fail(String.Format("Endpoint '{0}' is no longer checked in AD tree!", endpoint.Name));
            UIMapStartScanWizard.ClickNextButton();

            // Scan Credentials step
            UIHelper.WaitForControl(QuickScanButton());
            UIMapActiveDirectory.AssertLoginTextBoxTextExpectedValues.UILoginWatermarkTextBoEditText = DataHelper.Username;
            UIMapActiveDirectory.AssertLoginTextBoxText();
            TestContext.WriteLine(String.Format("Login '{0}' is shown correctly.", DataHelper.Username));
            UIMapStartScanWizard.ClickBackButton();
            TestContext.WriteLine("Back to Select Computers step...");
            UIHelper.WaitForControl(AdTree());
            UIMapStartScanWizard.ClickNextButton();
            TestContext.WriteLine("Returned to Scan Credentials step...");
            UIMapActiveDirectory.AssertLoginTextBoxTextExpectedValues.UILoginWatermarkTextBoEditText = DataHelper.Username;
            UIMapActiveDirectory.AssertLoginTextBoxText();
            TestContext.WriteLine(String.Format("Login '{0}' is shown correctly.", DataHelper.Username));
            UIMapStartScanWizard.ClickQuickScanButton();
            TestContext.WriteLine("Quick Scan button clicked...");

            WaitForScanCompleted(ScanType.Quick);
            //UIMapScanCompletedWindow.ClickCloseCrossButton();
            TestContext.WriteLine("Scan completed...");
            //UIHelper.WaitForControlIsClickable(StartScanButton());
            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickActiveDirectoryButton();
            UIHelper.WaitForControlIsClickable(NextButton());

            // Active Directory Credentials step after scan
            TestContext.WriteLine("Returned to Active Directory Credentials step after scan...");
            UIMapActiveDirectory.AssertDomainNameTextBoxTextExpectedValues.UIDomainNameWatermarkTEditText = domainName;
            UIMapActiveDirectory.AssertDomainNameTextBoxText();
            TestContext.WriteLine(String.Format("Domain Name '{0}' preserved.", domainName));
            UIMapActiveDirectory.AssertDomainAdministratorTextBoxTextExpectedValues.UIDomainAdministratorWEditText = DataHelper.Username;
            UIMapActiveDirectory.AssertDomainAdministratorTextBoxText();
            TestContext.WriteLine(String.Format("Domain Administrator '{0}' preserved.", DataHelper.Username));
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, TestContext);
            InputPassword(DataHelper.Password);
            UIMapStartScanWizard.ClickNextButton();

            // Select Computers step after scan
            UIHelper.WaitForControl(AdTree());
            TestContext.WriteLine("Returned to Select Computers step after scan...");
            ExpandAdRow(Settings.General.Default.AdTreeComputersElement);
            WaitForDomainComputer(endpoint);
            if (!IsDomainComputerChecked(endpoint)) TestContext.WriteLine(String.Format("Endpoint '{0}' is not checked in AD tree.", endpoint.Name));
            else Assert.Fail(String.Format("Endpoint '{0}' remains checked in AD tree!", endpoint.Name));
            CheckAdRow(endpoint.Name);
            UIMapStartScanWizard.ClickNextButton();

            // Scan Credentials step after scan
            UIHelper.WaitForControl(QuickScanButton());
            TestContext.WriteLine("Returned to Scan Credentials step after scan...");
            UIMapActiveDirectory.AssertLoginTextBoxTextExpectedValues.UILoginWatermarkTextBoEditText = DataHelper.Username;
            UIMapActiveDirectory.AssertLoginTextBoxText();
            TestContext.WriteLine(String.Format("Login '{0}' is shown correctly.", DataHelper.Username));
            UIMapStartScanWizard.ClickQuickScanButton();
            TestContext.WriteLine("Quick Scan button clicked...");
            WaitForScanCompleted(ScanType.Quick);
            //UIMapScanCompletedWindow.ClickCloseCrossButton();
            TestContext.WriteLine("Scan completed...");
        }
        
        [TestMethod]
        public void InvalidInputHandlingInWorkgroupTest()
        {
            string workgroup = SysHelper.GetWorkgroupName();

            //UIMapMain.ClickScanNowButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControlIsClickable(NextButton());

            // Workgroup
            UIMapWorkgroup.SelectUseKnownWorkgroupRadioButton();
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyWorkgroup, TestContext);            
            InputWorkgroup(workgroup);
            UIMapStartScanWizard.ClickNextButton();

            // Workgroup computer
            UIHelper.WaitForControl(WorkgroupTreeItem(workgroup), Settings.WaitingTime.Default.WorkgroupDiscoveryWaitTime);
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyComputer, TestContext);
            CheckWorkgroup(workgroup);
            UIMapStartScanWizard.ClickNextButton();

            // Login
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyLogin, TestContext);
            InputScanLogin(DataHelper.Username);

            // Password
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, TestContext);
            InputPassword(DataHelper.Password);
            UIMapStartScanWizard.ClickBackButton();
            UIHelper.WaitForControl(NextButton());
            UIMapStartScanWizard.ClickBackButton();

            // Non-existent workgroup
            string error = "error";
            InputWorkgroup(workgroup.Remove(workgroup.Length - error.Length) + error);
            UIMapStartScanWizard.ClickNextButton();
            UIHelper.WaitForControlIsClickable(ErrorTextBlock(), Settings.WaitingTime.Default.WorkgroupDiscoveryWaitTime);
            AssertErrorText(Settings.Errors.Default.WorkgroupNotFound, TestContext);
        }

        [TestMethod]
        public void InvalidInputHandlingInActiveDirectoryTest()
        {
            string domainName = SysHelper.GetDomainName();
            Computer endpoint = DataHelper.GetEndpoint();

            //UIMapMain.ClickScanNowButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickActiveDirectoryButton();
            UIHelper.WaitForControlIsClickable(NextButton());

            // Domain Name
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyDomainName, TestContext);            
            InputDomainName(domainName);

            // Domain Administrator
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyDomainAdministrator, TestContext);            
            InputDomainAdministrator(DataHelper.Username);

            // Password
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, TestContext);
            InputPassword(DataHelper.Password);
            UIMapStartScanWizard.ClickNextButton();

            // Domain computer
            UIHelper.WaitForControl(AdTree());
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyComputer, TestContext);
            ExpandAdRow(Settings.General.Default.AdTreeComputersElement);
            CheckAdRow(endpoint.Name);
            UIMapStartScanWizard.ClickNextButton();
            UIHelper.WaitForControl(QuickScanButton());

            /* 
            // Credentials input does not function here
            // Login
            InputScanLogin(String.Empty);
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyLogin, TestContext);

            // Password
            InputScanLogin(DataHelper.Username);
            InputPassword(String.Empty);
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, TestContext);
            */
            UIMapStartScanWizard.ClickBackButton();
            UIHelper.WaitForControl(NextButton());
            UIMapStartScanWizard.ClickBackButton();

            // Non-existent domain
            string nonExistentDomainName = domainName + ".error";
            InputDomainName(nonExistentDomainName);
            UIMapStartScanWizard.ClickNextButton();
            UIHelper.WaitForControlIsClickable(ErrorTextBlock(), Settings.WaitingTime.Default.DomainDiscoveryWaitTime);
            AssertErrorText(String.Format(Settings.Errors.Default.DomainNotFound, nonExistentDomainName), TestContext);
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