using System;
using System.Linq;
using System.Diagnostics;
using CodedUITestFAT.Helpers;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;


namespace CodedUITestFAT
{
    [CodedUITest]
    class CodedUITestActiveDirectory : CodedUITestCommon
    {
        public CodedUITestActiveDirectory(){}

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

        #region Controls
        private WpfTree AdTree()
        {
            return UIMapActiveDirectory.UIForensicAnalysisToolWindow.UIActiveDirectoryViewCustom.UIAdTreeTree;
        }

        private WpfEdit DomainNameTextBox()
        {
            return UIMapActiveDirectory.UIForensicAnalysisToolWindow.UIThisCustom.UIDomainNameWatermarkTEdit;
        }


        #endregion

        #region Help Methods

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
            WpfTreeItem domainTreeItem = UIMapActiveDirectory.UIForensicAnalysisToolWindow.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDevTreeItem;
            domainTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name] = SysHelper.GetDomainName(true);

            WpfTreeItem computersTreeItem = domainTreeItem.GetChildren().FirstOrDefault(i => i is WpfTreeItem && i.Name == Settings.General.Default.AdTreeComputersElement) as WpfTreeItem;
            if (computersTreeItem == null) throw new Exception(String.Format("Tree item '{0}' not found!", Settings.General.Default.AdTreeComputersElement));
            WpfTreeItem endpointTreeItem = computersTreeItem.GetChildren().FirstOrDefault(i => i is WpfTreeItem && i.Name == endpoint.Name.ToUpper()) as WpfTreeItem;
            if (endpointTreeItem == null) throw new Exception(String.Format("Tree item '{0}' not found!", endpoint.Name.ToUpper()));
            WpfCheckBox endpointCheckBox = endpointTreeItem.GetChildren().FirstOrDefault(i => i is WpfCheckBox) as WpfCheckBox;

            return endpointCheckBox.Checked;
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

        /// <summary> Expands a given row in Active Directory tree. In accordance with DeploymentItem.cs. </summary>
        /// <param name="rowName">Set a unique row name.</param>
        private void ExpandAdRow(string rowName)
        {
            UIMapActiveDirectory.UIForensicAnalysisToolWindow.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDevTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name] = SysHelper.GetDomainName(true);
            UIMapActiveDirectory.UIForensicAnalysisToolWindow.UIActiveDirectoryViewCustom.UIAdTreeTree.UIDevTreeItem.UIComputersTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name] = rowName;

            UIMapActiveDirectory.ExpandAdRow();
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
                }
                catch (Exception) { }
                if ((int)timer.ElapsedMilliseconds > waitTime)
                    throw new Exception(String.Format("Domain computer '{0}' is not available at AD tree after {1} seconds!", endpoint.Name, waitTime / 1000));
            }
            timer.Stop();
        }

        #endregion

        #region Test Methods
        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        [TestCategory("ActiveDirectory")]
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

            TestContext.WriteLine(string.Format("Quick Scan started at endpoint '{0}' from domain '{1}'...", endpoint.Name, domainName));

            WaitForScanCompleted(ScanType.Quick);

            UIHelper.GroupByFiles();
            UIHelper.ClickRow(malware);
            TestContext.WriteLine(string.Format("Malicious file {0} found at the results screen.", malware));
            UIHelper.ClickRow(unknownFile);
            TestContext.WriteLine(string.Format("Unknown file {0} found at the results screen.", unknownFile));

            UIHelper.ClickRow(malware, MouseClick.DoubleLeft);
            UIMapDetailsWindow.AssertThreatNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = malware.Name;
            UIMapDetailsWindow.AssertThreatNameTextBlockText();
            TestContext.WriteLine(string.Format("Malicious file name {0} found at threat Details screen.", malware.Name));
            UIMapDetailsWindow.ClickCloseButton();

            UIHelper.ClickRow(unknownFile, MouseClick.DoubleLeft);
            UIMapDetailsWindow.AssertThreatNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = unknownFile.Name;
            UIMapDetailsWindow.AssertThreatNameTextBlockText();
            TestContext.WriteLine(string.Format("Unknown file name {0} found at threat Details screen.", unknownFile.Name));
            UIMapDetailsWindow.ClickCloseButton();
        }


        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        [TestCategory("ActiveDirectory")]
        public void ScanningWizardNavigationActiveDirectoryTest()
        {
            string domainName = SysHelper.GetDomainName();
            Computer endpoint = DataHelper.GetEndpoint();

            UIHelper.WaitForControlIsClickable(SkipDiscoveryButton());
            UIMapDiscovery.ClickSkipDiscoveryButton();

            SysHelper.DisinfectForQuickScan(endpoint, TestContext);

            UIHelper.WaitForControlIsClickable(CustomScanButton());
            UIMapDiscovery.ClickCustomScanButton();

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
            TestContext.WriteLine(string.Format("Domain Name '{0}' preserved.", domainName));
            UIMapActiveDirectory.AssertDomainAdministratorTextBoxTextExpectedValues.UIDomainAdministratorWEditText = DataHelper.Username;
            UIMapActiveDirectory.AssertDomainAdministratorTextBoxText();
            TestContext.WriteLine(string.Format("Domain Administrator '{0}' preserved.", DataHelper.Username));
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
            if (IsDomainComputerChecked(endpoint)) TestContext.WriteLine(string.Format("Endpoint '{0}' remains checked in AD tree.", endpoint.Name));
            else Assert.Fail(string.Format("Endpoint '{0}' is no longer checked in AD tree!", endpoint.Name));
            UIMapStartScanWizard.ClickNextButton();

            // Scan Credentials step
            UIHelper.WaitForControl(QuickScanButton());
            UIMapActiveDirectory.AssertLoginTextBoxTextExpectedValues.UILoginWatermarkTextBoEditText = DataHelper.Username;
            UIMapActiveDirectory.AssertLoginTextBoxText();
            TestContext.WriteLine(string.Format("Login '{0}' is shown correctly.", DataHelper.Username));
            UIMapStartScanWizard.ClickBackButton();
            TestContext.WriteLine("Back to Select Computers step...");
            UIHelper.WaitForControl(AdTree());
            UIMapStartScanWizard.ClickNextButton();
            TestContext.WriteLine("Returned to Scan Credentials step...");
            UIMapActiveDirectory.AssertLoginTextBoxTextExpectedValues.UILoginWatermarkTextBoEditText = DataHelper.Username;
            UIMapActiveDirectory.AssertLoginTextBoxText();
            TestContext.WriteLine(string.Format("Login '{0}' is shown correctly.", DataHelper.Username));
            UIMapStartScanWizard.ClickQuickScanButton();
            TestContext.WriteLine("Quick Scan button clicked...");

            WaitForScanCompleted(ScanType.Quick);
            TestContext.WriteLine("Scan completed...");

            UIMapResultsWindow.ClickNewCustomScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickActiveDirectoryButton();
            UIHelper.WaitForControlIsClickable(NextButton());

            // Active Directory Credentials step after scan
            TestContext.WriteLine("Returned to Active Directory Credentials step after scan...");
            UIMapActiveDirectory.AssertDomainNameTextBoxTextExpectedValues.UIDomainNameWatermarkTEditText = domainName;
            UIMapActiveDirectory.AssertDomainNameTextBoxText();
            TestContext.WriteLine(string.Format("Domain Name '{0}' preserved.", domainName));
            UIMapActiveDirectory.AssertDomainAdministratorTextBoxTextExpectedValues.UIDomainAdministratorWEditText = DataHelper.Username;
            UIMapActiveDirectory.AssertDomainAdministratorTextBoxText();
            TestContext.WriteLine(string.Format("Domain Administrator '{0}' preserved.", DataHelper.Username));
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, TestContext);
            InputPassword(DataHelper.Password);
            UIMapStartScanWizard.ClickNextButton();

            // Select Computers step after scan
            UIHelper.WaitForControl(AdTree());
            TestContext.WriteLine("Returned to Select Computers step after scan...");
            ExpandAdRow(Settings.General.Default.AdTreeComputersElement);
            WaitForDomainComputer(endpoint);
            if (!IsDomainComputerChecked(endpoint)) TestContext.WriteLine(string.Format("Endpoint '{0}' is not checked in AD tree.", endpoint.Name));
            else Assert.Fail(string.Format("Endpoint '{0}' remains checked in AD tree!", endpoint.Name));
            CheckAdRow(endpoint.Name);
            UIMapStartScanWizard.ClickNextButton();

            // Scan Credentials step after scan
            UIHelper.WaitForControl(QuickScanButton());
            TestContext.WriteLine("Returned to Scan Credentials step after scan...");
            UIMapActiveDirectory.AssertLoginTextBoxTextExpectedValues.UILoginWatermarkTextBoEditText = DataHelper.Username;
            UIMapActiveDirectory.AssertLoginTextBoxText();
            TestContext.WriteLine(string.Format("Login '{0}' is shown correctly.", DataHelper.Username));
            UIMapStartScanWizard.ClickQuickScanButton();
            TestContext.WriteLine("Quick Scan button clicked...");
            WaitForScanCompleted(ScanType.Quick);
            TestContext.WriteLine("Scan completed...");
        }

        [TestMethod]
        [TestCategory("ActiveDirectory")]
        public void InvalidInputHandlingInActiveDirectoryTest()
        {
            string domainName = SysHelper.GetDomainName();
            Computer endpoint = DataHelper.GetEndpoint();

            UIHelper.WaitForControlIsClickable(SkipDiscoveryButton());
            UIMapDiscovery.ClickSkipDiscoveryButton();
            UIHelper.WaitForControlIsClickable(CustomScanButton());
            UIMapDiscovery.ClickCustomScanButton();
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
            UIMapStartScanWizard.ClickBackButton();
            UIHelper.WaitForControl(NextButton());
            UIMapStartScanWizard.ClickBackButton();

            // Non-existent domain
            string nonExistentDomainName = domainName + ".error";
            InputDomainName(nonExistentDomainName);
            UIMapStartScanWizard.ClickNextButton();
            UIHelper.WaitForControlIsClickable(ErrorTextBlock(), Settings.WaitingTime.Default.DomainDiscoveryWaitTime);
            AssertErrorText(string.Format(Settings.Errors.Default.DomainNotFound, nonExistentDomainName), TestContext);
        }
        #endregion

    }
}
