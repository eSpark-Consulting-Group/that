using System.Linq;
using CodedUITestFAT.Helpers;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestFAT.UIMaps.StartScanWizard.UIMapWorkgroupClasses;

namespace CodedUITestFAT
{
    [CodedUITest]
    class CodedUITestWorkgroup : CodedUITestCommon
    {

        public CodedUITestWorkgroup() {
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
        #endregion

        #region Controls
        private WpfTreeItem WorkgroupTreeItem(string workgroupName)
        {
            UIMapWorkgroup.UIForensicAnalysisToolWindow.UIWorkgroupViewCustom.UIWorkgroupTreeTree.UIWorkgroupTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name]
                        = workgroupName;
            return UIMapWorkgroup.UIForensicAnalysisToolWindow.UIWorkgroupViewCustom.UIWorkgroupTreeTree.UIWorkgroupTreeItem;
        }
        #endregion

        #region Helper Methods

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

        /// <summary> Inputs Workgroup, if necessary. </summary>
        /// <param name="workgroupName">Set a workgroup name to input.</param>
        private void InputWorkgroup(string workgroupName)
        {
            WpfEdit workgroupEdit = UIMapWorkgroup.UIForensicAnalysisToolWindow.UIWorkgroupViewCustom.UIWorkgroupWatermarkTeEdit;
            if (workgroupEdit.Text == Settings.General.Default.EmptyWatermarkTextBoxText || workgroupEdit.Text != workgroupName) workgroupEdit.Text = workgroupName;
        }
        #endregion

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

        #region Test methods   
        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        [TestCategory("Smoke")]
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
            TestContext.WriteLine(string.Format("Quick Scan started in Workgroup '{0}'...", workgroupName));

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
        [TestCategory("Smoke"), TestCategory("UiDefaults")]
        public void ScanningWizardNavigationWorkgroupTest()
        {
            string workgroupName = SysHelper.GetWorkgroupName();

            UIHelper.WaitForControlIsClickable(SkipDiscoveryButton());
            UIMapDiscovery.ClickSkipDiscoveryButton();

            SysHelper.DisinfectForQuickScan(DataHelper.GetEndpoint(), TestContext);

            UIHelper.WaitForControlIsClickable(CustomScanButton());
            UIMapDiscovery.ClickCustomScanButton();

            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControlIsEnabled(NextButton());

            // Specify Workgroup step
            UIMapWorkgroup.SelectUseKnownWorkgroupRadioButton();
            UIMapStartScanWizard.ClickBackButton();
            TestContext.WriteLine("Back to Start Scan Wizard...");
            UIHelper.WaitForControl(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControl(NextButton());
            TestContext.WriteLine("Returned to Specify Workgroup step...");
            UIMapWorkgroup.AssertWorkgroupTextBoxTextExpectedValues.UIWorkgroupWatermarkTeEditText = workgroupName;
            UIMapWorkgroup.AssertWorkgroupTextBoxText();
            TestContext.WriteLine(string.Format("Workgroup name '{0}' is preserved at Specify Workgroup step.", workgroupName));
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
            if (IsWorkgroupChecked(workgroupName)) TestContext.WriteLine(string.Format("Workgroup '{0}' is still selected at Select Computers step.", workgroupName));
            else Assert.Fail(string.Format("Workgroup '{0}' is not selected at Select Computers step!", workgroupName));
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
            TestContext.WriteLine(string.Format("Login '{0}' is preserved.", DataHelper.Username));
            UIMapStartScanWizard.ClickQuickScanButton();
            TestContext.WriteLine("Quick Scan button clicked...");

            WaitForScanCompleted(ScanType.Quick);


            UIMapResultsWindow.ClickNewCustomScanButton();

            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControl(NextButton());

            // Specify Workgroup step after scan
            TestContext.WriteLine("Returned to Specify Workgroup step after scan...");
            UIMapWorkgroup.AssertWorkgroupTextBoxTextExpectedValues.UIWorkgroupWatermarkTeEditText = workgroupName;
            UIMapWorkgroup.AssertWorkgroupTextBoxText();
            TestContext.WriteLine(string.Format("Workgroup name '{0}' is preserved at Specify Workgroup step.", workgroupName));
            UIMapStartScanWizard.ClickNextButton();

            // Select Computers step after scan
            TestContext.WriteLine("Returned to Select Computers step after scan...");
            UIHelper.WaitForControl(WorkgroupTreeItem(workgroupName), Settings.WaitingTime.Default.WorkgroupDiscoveryWaitTime);
            UIHelper.WaitForControlIsEnabled(WorkgroupTreeItem(workgroupName));
            if (!IsWorkgroupChecked(workgroupName)) TestContext.WriteLine(string.Format("Workgroup '{0}' is not selected at Select Computers step.", workgroupName));
            else Assert.Fail(string.Format("Workgroup '{0}' is still selected at Select Computers step!", workgroupName));
            CheckWorkgroup(workgroupName);
            UIMapStartScanWizard.ClickNextButton();

            // Scan Credentials step after scan            
            TestContext.WriteLine("Returned to Scan Credentials step after scan...");
            UIMapWorkgroup.AssertLoginTextBoxTextExpectedValues.UILoginWatermarkTextBoEditText = DataHelper.Username;
            UIMapWorkgroup.AssertLoginTextBoxText();
            TestContext.WriteLine(string.Format("Login '{0}' is preserved.", DataHelper.Username));
            UIMapStartScanWizard.ClickQuickScanButton();
            TestContext.WriteLine("Quick Scan button clicked...");
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, TestContext);
            UIMapStartScanWizard.ClickCloseScanWizardButton();
        }


        [TestMethod]
        [TestCategory("Smoke"), TestCategory("UiDefaults")]
        public void InvalidInputHandlingInWorkgroupTest()
        {
            string workgroup = SysHelper.GetWorkgroupName();

            UIHelper.WaitForControlIsClickable(SkipDiscoveryButton());
            UIMapDiscovery.ClickSkipDiscoveryButton();
            UIHelper.WaitForControlIsClickable(CustomScanButton());
            UIMapDiscovery.ClickCustomScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickWorkgroupButton();
            UIHelper.WaitForControlIsClickable(NextButton());

            // Workgroup
            UIMapWorkgroup.SelectUseKnownWorkgroupRadioButton();
            UIMapWorkgroup.DeleteWorkgroupName();
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
        #endregion
    }
}
