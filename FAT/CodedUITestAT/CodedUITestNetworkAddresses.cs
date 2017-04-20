using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestFAT.UIMaps.StartScanWizard.UIMapNetworkAddressesClasses;

namespace CodedUITestFAT
{
    /// <summary> Tests for Network Addresses branch. </summary>
    [CodedUITest]
    public sealed class CodedUITestNetworkAddresses : CodedUITestCommon
    {
        public CodedUITestNetworkAddresses()
        {
        }       
        #region Initialize and cleanup

        [TestInitialize]
        public void ATTestInitialize()
        {
            SysHelper.StartProduct(DataHelper.GetEndpoint());

            CodedUITestEula useEula = new CodedUITestEula();
            useEula.UIMapEula.ClickIAcceptButton();

            UIHelper.WaitForControlIsEnabled(SkipDiscoveryButton());
            UIMapDiscovery.ClickSkipDiscoveryButton();
            UIMapDiscovery.ClickCustomScanButton();
            //UIHelper.WaitForControl(NetworkAddressButton());
            UIMapStartScanWizard.ClickNetworkAddressButton();
            //UIHelper.WaitForControl(NextButton());
            //UIHelper.WaitForControlIsEnabled(NextButton());            
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

        #region Start Scan Wizard
        #endregion

        #region Network Addresses

        private WpfList NetworkAddressesList()
        {
            return UIMapNetworkAddresses.UIForensicAnalysisToolWindow.UINetworkAddressesViewCustom.UITextEntryListViewCustom.UINetworkAddressItemsLList;
        }


        internal WpfButton RemoveAllButton()
        {
            return UIMapNetworkAddresses.UIForensicAnalysisToolWindow.UINetworkAddressesViewCustom.UITextEntryListViewCustom.UIRemoveAllButton;
        }

        internal WpfButton StopScanButton()
        {
            return UIMapMain.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIPleaseWaitProgressBar.UIStopScanButtonButton;
        }

        internal WpfEdit SearchField()
        {
            return UIMapMain.UIForensicAnalysisToolWindow.UISearchControlCustom.UISearchWatermarkTextBEdit;
        }
     
        #endregion

        #endregion

        #region Local helper methods

        /// <summary> Starts a scan of a given type at indicated endpoints via Network Addresses branch using set credentials. Starts at Network Addresses. </summary>
        /// <param name="scanType">Set a scan type.</param>
        /// <param name="username">Set a username.</param>
        /// <param name="password">Set a password.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        /// <param name="endpoints">Set endpoints.</param>
        private void StartEndpointScan(ScanType scanType, string username, string password, TestContext testContext, params Computer[] endpoints)
        {
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

        /// <summary> Starts a scan of a given type at indicated endpoint via Network Addresses branch. Starts at Network Addresses. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        /// <param name="scanType">Set a scan type.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        internal void StartEndpointScan(Computer endpoint, ScanType scanType, TestContext testContext)
        {
            StartEndpointScan(scanType, DataHelper.Username, DataHelper.Password, testContext, endpoint);
        }

        /// <summary> Cleans Network Addresses list. </summary>
        private void CleanNetworkAddressesList()
        {
            if (NetworkAddressesList().Items.Count > 0) UIMapNetworkAddresses.ClickRemoveAllButton();
        }

        /// <summary> Stops a scan. </summary>
        private void StopScanning(TestContext testContext)
        {
            UIMapMain.ClickStopScanButton();
            UIHelper.WaitForControl(YesDialogButton());
            UIMapStopScanWindow.ClickYesButton();
            testContext.WriteLine("The scanning has been canceled.");
        }


        /// <summary> Verify is tested host in the scanning list</summary>
        /// <param name="endpointName">Endpoint name.</param>
        private void VerifyAdressListContainsHost(string endpointName)
        {
            if ((from x in NetworkAddressesList().Items
                 where x.Name.Equals(endpointName)
                 select new UITestControl()).Any())
                TestContext.WriteLine(String.Format("Endpoint name '{0}' is still present in Network Addresses list.", endpointName));
            else
                Assert.Fail(String.Format("Endpoint name '{0}' is no longer present in Network Addresses list!", endpointName));
        }

        #endregion

        #region Test methods

        //TODO need to repair
        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void EndpointQuickScanAndValkyrieUriCheckTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            Malware malware = SysHelper.InfectForScan(endpoint);
            UnknownFile unknownFile = SysHelper.AddUnknownFileForScan(endpoint);

            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);

            UIHelper.FindThreat(malware, TestContext);
            UIHelper.FindThreat(unknownFile, TestContext);

            // Clicks Valkyrie link and checks the respective browser window URI
            string expectedValkyrieUri = Settings.General.Default.ValkyrieUri;
            UIHelper.WaitForControl(DetailedScanResultsButton());
            UIMapResultsWindow.ClickDetailedScanResultsButton();
            TestContext.WriteLine("Detailed Scan Results button clicked...");
            UIHelper.AssertBrowserUri(expectedValkyrieUri, TestContext);
        }

        //TODO need to repair
        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void QuickScanOfIpRangeTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            Computer server = new Computer();                   
            IPAddress internalAddress = SysHelper.GetLocalIpAddress(true);
            string ipRange = String.Format("{0}-{1}", internalAddress.ChangeLastByte(0), internalAddress.ChangeLastByte(255));           
            UnknownFile unknownFile = SysHelper.AddUnknownFileForScan(endpoint);
            Malware malware = SysHelper.InfectForScan(endpoint);

            CleanNetworkAddressesList();
            UIMapNetworkAddresses.InputNetworkAddressParams.UINetworkAddressTextBoEditText = ipRange;
            UIMapNetworkAddresses.InputNetworkAddress();
            UIMapNetworkAddresses.ClickAddButton();
            TestContext.WriteLine(String.Format("IP range {0} added to the addresses list.", ipRange));
            UIMapStartScanWizard.ClickNextButton();

            UIHelper.WaitForControl(QuickScanButton(), Settings.WaitingTime.Default.EndpointResolvingWaitTime);
            UIHelper.WaitForControlIsEnabled(QuickScanButton(), Settings.WaitingTime.Default.EndpointResolvingWaitTime);
            InputScanLogin(DataHelper.Username);
            InputPassword(DataHelper.Password);
            UIMapStartScanWizard.ClickQuickScanButton();

            UIHelper.WaitForControl(CloseSummaryWindowButton(), Settings.WaitingTime.Default.QuickScanRangeWaitTime);
            UIHelper.WaitForControlIsClickable(CloseSummaryWindowButton());
            UIMapResultsWindow.ClickCloseSummaryWindowButton();

            // Verifies the scan results
            UIHelper.ClickRow(server);
            TestContext.WriteLine(String.Format("Computer '{0}' found at the scan results screen.", server.Name));
            UIHelper.ClickRow(endpoint);
            TestContext.WriteLine(String.Format("Computer '{0}' found at the scan results screen.", endpoint.Name));
            UIHelper.FindThreat(malware, TestContext);
            UIHelper.FindThreat(unknownFile, TestContext);
        }

        //TODO need to repair
        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void QuickScanOfSubnetTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            Computer server = new Computer();
            IPAddress internalAddress = SysHelper.GetLocalIpAddress(true);
            string subnet = String.Format("{0}/{1}", internalAddress.ChangeLastByte(0), 24);
            UnknownFile unknownFile = SysHelper.AddUnknownFileForScan(endpoint);
            Malware malware = SysHelper.InfectForScan(endpoint);

            CleanNetworkAddressesList();
            UIMapNetworkAddresses.InputNetworkAddressParams.UINetworkAddressTextBoEditText = subnet;
            UIMapNetworkAddresses.InputNetworkAddress();
            UIMapNetworkAddresses.ClickAddButton();
            TestContext.WriteLine(String.Format("Subnet {0} added to the addresses list.", subnet));
            UIMapStartScanWizard.ClickNextButton();

            UIHelper.WaitForControl(QuickScanButton(), Settings.WaitingTime.Default.EndpointResolvingWaitTime);
            UIHelper.WaitForControlIsEnabled(QuickScanButton(), Settings.WaitingTime.Default.EndpointResolvingWaitTime);
            InputScanLogin(DataHelper.Username);
            InputPassword(DataHelper.Password);
            UIMapStartScanWizard.ClickQuickScanButton();

            UIHelper.WaitForControl(CloseSummaryWindowButton(), Settings.WaitingTime.Default.QuickScanRangeWaitTime);
            UIHelper.WaitForControlIsClickable(CloseSummaryWindowButton());
            UIMapResultsWindow.ClickCloseSummaryWindowButton();

            // Verifies the scanning results
            UIHelper.ClickRow(server);
            TestContext.WriteLine(String.Format("Computer '{0}' found at the scan results screen.", server.Name));
            UIHelper.ClickRow(endpoint);
            TestContext.WriteLine(String.Format("Computer '{0}' found at the scan results screen.", endpoint.Name));
            UIHelper.FindThreat(malware, TestContext);
            UIHelper.FindThreat(unknownFile, TestContext);
        }

        //TODO need to repair
        [TestMethod]
        public void StopFullScanTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            string scannerServiceName = Settings.Scanner.Default.ServiceName;
            string scannerPath = String.Format(@"\\{0}\{1}\{2}", endpoint.Name, DataHelper.GetEndpointRoot(true), Settings.Scanner.Default.Directory);

            StartEndpointScan(endpoint, ScanType.Full, TestContext);

            //UIHelper.WaitForControlIsEnabled(StopScanButton());
            //UIHelper.WaitForControlIsClickable(StopScanButton());
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

        //TODO need to repair
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

        //TODO need to repair
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

        //TODO need to repair
        [TestMethod]
        public void SearchAtResultsScreenTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            SysHelper.InfectForScan(endpoint);
            SysHelper.AddUnknownFileForScan(endpoint);
            string partialNameInput = endpoint.Name.Substring(1, (int)((endpoint.Name.Length - 1) / 2));
            string error = "error";
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            //UIMapScanCompletedWindow.ClickYesButton();

            //InputCamCredentials(TestContext);
            //WaitForValkyrieResults(TestContext);

            // Searches with a correct input
            UIMapMain.InputTextToSearchBoxParams.UISearchWatermarkTextBEditSendKeys = partialNameInput;
            UIMapMain.InputTextToSearchBox();
            TestContext.WriteLine("A part of endpoint name is entered to the searchbox at 'Valkyrie analysis results' tab.");
            UIHelper.WaitForResultsRow(endpoint, TestContext);
            UIMapMain.ClickClearSearchTextButton();

            // Searches with an incorrect input
            UIMapResultsWindow.GoToValkyrieAnalysisResultsTab();
            TestContext.WriteLine("An incorrect search query is entered to the searchbox at 'Valkyrie analysis results' tab.");
            UIMapMain.InputTextToSearchBoxParams.UISearchWatermarkTextBEditSendKeys = partialNameInput + error;
            UIMapMain.InputTextToSearchBox();
            try
            {
                UIHelper.WaitForResultsRow(endpoint, TestContext);
                Assert.Fail(String.Format("Endpoint '{0}' is shown at 'Valkyrie analysis results' tab after an incorrect search query!", endpoint.Name));
            }
            catch (UITestControlNotFoundException)
            {
                TestContext.WriteLine(String.Format("Endpoint '{0}' is not shown at 'Valkyrie analysis results' tab after an incorrect search query.", endpoint.Name));
            }
            UIMapMain.ClickClearSearchTextButton();

            UIHelper.WaitForResultsRow(endpoint, TestContext); // The endpoint should appear again

            // Searches with a correct input
            UIMapResultsWindow.GoToScanResultsTab();
            UIMapMain.InputTextToSearchBoxParams.UISearchWatermarkTextBEditSendKeys = partialNameInput;
            UIMapMain.InputTextToSearchBox();
            TestContext.WriteLine("A part of endpoint name is entered to the searchbox at 'Scanning results' tab.");
            UIHelper.WaitForResultsRow(endpoint, TestContext);
            UIMapMain.ClickClearSearchTextButton();

            // Searches with an incorrect input
            UIMapResultsWindow.GoToScanResultsTab();
            UIMapMain.InputTextToSearchBoxParams.UISearchWatermarkTextBEditSendKeys = partialNameInput + error;
            UIMapMain.InputTextToSearchBox();
            TestContext.WriteLine("An incorrect search query is entered to the searchbox at 'Scan results' tab.");
            try
            {
                UIHelper.WaitForResultsRow(endpoint, TestContext);
                Assert.Fail(String.Format("Endpoint '{0}' is shown on 'Scan results' tab after an incorrect search query!", endpoint.Name));
            }
            catch (UITestControlNotFoundException)
            {
                TestContext.WriteLine(String.Format("Endpoint '{0}' is not shown on 'Scan results' tab after an incorrect search query.", endpoint.Name));
            }
            UIMapMain.ClickClearSearchTextButton();

            UIHelper.WaitForResultsRow(endpoint, TestContext); // The endpoint should appear again
        }

        //TODO need to repair
        [TestMethod]
        public void VerifyThreatDetailsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            Malware malware = SysHelper.InfectForScan(endpoint);
            SysHelper.AddUnknownFileForScan(endpoint);

            // Performs a scan
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            //UIHelper.WaitForControlIsClickable(StartScanButton());

            // Opens a threat details via context menu
            UIHelper.ClickRow(endpoint);
            UIHelper.ClickRow(malware);
            UIHelper.ClickRow(malware, MouseClick.Right);
            UIMapResultsWindow.SelectDetailsByComputersContextMenuItem();
            //UIHelper.WaitForControl(CloseDetailsButton());

            // Checks all the threat details
            UIMapDetailsWindow.AssertThreatNameTextBlockTextExpectedValues.UIItem91exestealthexeTextDisplayText = malware.Name;
            UIMapDetailsWindow.AssertThreatNameTextBlockText();
            TestContext.WriteLine(String.Format("Threat name is shown correctly: '{0}'.", malware.Name));

            UIMapDetailsWindow.AssertFileNameValueTextBoxTextExpectedValues.UIFileNameValueTextBoxEditText = malware.Name;
            UIMapDetailsWindow.AssertFileNameValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'File Name' parameter is shown correctly: '{0}'.", malware.Name));

            UIMapDetailsWindow.AssertVerdictValueTextBoxTextExpectedValues.UIVerdictValueTextBoxEditText = malware.FileVerdict.ToString();
            UIMapDetailsWindow.AssertVerdictValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Verdict' parameter is shown correctly: '{0}'.", malware.FileVerdict));

            UIMapDetailsWindow.AssertSizeValueTextBoxTextExpectedValues.UISizeValueTextBoxEditText = malware.DisplaySize;
            UIMapDetailsWindow.AssertSizeValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Size' parameter is shown correctly: '{0}'.", malware.DisplaySize));

            UIMapDetailsWindow.AssertCreatedValueTextBoxTextExpectedValues.UICreatedValueTextBoxEditText = malware.Created;
            UIMapDetailsWindow.AssertCreatedValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Created' parameter is shown correctly: '{0}'.", malware.Created));

            UIMapDetailsWindow.AssertModifiedValueTextBoxTextExpectedValues.UIModifiedValueTextBoxEditText = malware.Modified;
            UIMapDetailsWindow.AssertModifiedValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Modified' parameter is shown correctly: '{0}'.", malware.Modified));

            UIMapDetailsWindow.AssertAccessedValueTextBoxTextExpectedValues.UIAccessedValueTextBoxEditText = malware.Accessed;
            UIMapDetailsWindow.AssertAccessedValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Accessed' parameter is shown correctly: '{0}'.", malware.Accessed));

            UIMapDetailsWindow.AssertHashValueTextBoxTextExpectedValues.UIHashValueTextBoxEditText = malware.Sha1;
            UIMapDetailsWindow.AssertHashValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Hash' parameter is shown correctly: '{0}'.", malware.Sha1));

            UIMapDetailsWindow.AssertSignerValueTextBoxTextExpectedValues.UISignerValueTextBoxEditText = malware.Signer;
            UIMapDetailsWindow.AssertSignerValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Signer' parameter is shown correctly: '{0}'.", malware.Signer));

            UIMapDetailsWindow.AssertDescriptionValueTextBoxTextExpectedValues.UIDescriptionValueTextEditText = malware.Description;
            UIMapDetailsWindow.AssertDescriptionValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Description' parameter is shown correctly: '{0}'.", malware.Description));

            UIMapDetailsWindow.AssertVersionValueTextBoxTextExpectedValues.UIVersionValueTextBoxEditText = malware.Version;
            UIMapDetailsWindow.AssertVersionValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Version' parameter is shown correctly: '{0}'.", malware.Version));

            UIMapDetailsWindow.AssertCopyrightValueTextBoxTextExpectedValues.UICopyrightValueTextBoEditText = malware.Copyright;
            UIMapDetailsWindow.AssertCopyrightValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Copyright' parameter is shown correctly: '{0}'.", malware.Copyright));

            UIMapDetailsWindow.AssertProductValueTextBoxTextExpectedValues.UIProductValueTextBoxEditText = malware.Product;
            UIMapDetailsWindow.AssertProductValueTextBoxText();
            TestContext.WriteLine(String.Format("Threat 'Product' parameter is shown correctly: '{0}'.", malware.Product));

            UIMapDetailsWindow.ClickCloseButton();
            //UIHelper.WaitForControlIsClickable(StartScanButton());
        }

        //TODO need to repair
        [TestMethod]
        public void CheckScanResultPanelTest()
        {
            CodedUITestMain useMain = new CodedUITestMain();
            Computer endpoint = DataHelper.GetEndpoint();

            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            Malware malware = SysHelper.InfectForScan(endpoint);
            TestContext.WriteLine(String.Format("Malware '{0}' is copied to the endpoint '{1}'.", malware.Name, endpoint.Name));
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            WaitForNoInAnalysis();
            //UIMapScanCompletedWindow.ClickCloseCrossButton();
            useMain.AssertPanelCountersData(0, 1, 0, TestContext);

            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            UnknownFile unknown = SysHelper.AddUnknownFileForScan(endpoint);
            TestContext.WriteLine(String.Format("Unknown file '{0}' is copied to the endpoint '{1}'.", unknown.Name, endpoint.Name));
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            WaitForNoInAnalysis();
            //UIMapScanCompletedWindow.ClickCloseCrossButton();
            useMain.AssertPanelCountersData(1, 0, 0, TestContext);

            malware = SysHelper.InfectForScan(endpoint);
            TestContext.WriteLine(String.Format("Malware '{0}' is copied to the endpoint '{1}'.", malware.Name, endpoint.Name));
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            WaitForNoInAnalysis();
            //UIMapScanCompletedWindow.ClickCloseCrossButton();
            useMain.AssertPanelCountersData(1, 1, 0, TestContext);

            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            WaitForNoInAnalysis();
            //UIMapScanCompletedWindow.ClickCloseCrossButton();
            useMain.AssertPanelCountersData(0, 0, 0, TestContext);
        }

        //TODO need to repair
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

            UIHelper.FindThreat(malware, TestContext);
            UIHelper.FindThreat(unknownFile, TestContext);
        }

        //TODO need to repair
        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void VerifyThreatValkyrieDetailsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            UnknownFile unknownFile = SysHelper.AddUnknownFileForScan(endpoint);

            // Performs a scan
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            //UIMapScanCompletedWindow.ClickYesButton();
            //InputCamCredentials(TestContext);
            //WaitForValkyrieResults(TestContext);

            // Opens a threat details via context menu
            UIHelper.ClickRow(endpoint);
            UIHelper.ClickRow(unknownFile, MouseClick.DoubleLeft);
            //UIHelper.WaitForControl(CloseValkyrieDetailsButton());

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
            //UIHelper.WaitForControlIsClickable(StartScanButton());
        }

        //TODO need to repair
        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void CheckValkyrieAnalysisResultsPanelTest()
        {
            CodedUITestMain useMain = new CodedUITestMain();
            Computer endpoint = DataHelper.GetEndpoint();

            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            UnknownFile unknown = SysHelper.AddUnknownFileForScan(endpoint);
            TestContext.WriteLine(String.Format("Unknown file '{0}' is copied to the endpoint '{1}'.", unknown.Name, endpoint.Name));
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            WaitForNoInAnalysis();
            //UIMapScanCompletedWindow.ClickYesButton();
            //InputCamCredentials(TestContext);
            useMain.WaitForValkyrieResults(TestContext);
            UIHelper.ClickRow(endpoint);
            useMain.AssertPanelCountersData(0, 0, 1, TestContext);
            UIHelper.WaitForCellContent(ResultsScreen.ValkyrieAnalysis, unknown, Settings.ResultsValkyrie.Default.VerdictCellNumber, ValkyrieVerdict.NoThreatFound.ToUi(),
                                                                                                                TestContext, Settings.WaitingTime.Default.NoThreatFoundVerdictWaitTime);
            useMain.AssertPanelCountersData(1, 0, 0, TestContext);

            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            unknown = SysHelper.AddUnknownFileForScan(endpoint);
            TestContext.WriteLine(String.Format("Unknown file '{0}' is copied to the endpoint '{1}'.", unknown.Name, endpoint.Name));
            Malware malware = SysHelper.InfectForScan(endpoint);
            TestContext.WriteLine(String.Format("Malware '{0}' is copied to the endpoint '{1}'.", malware.Name, endpoint.Name));
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            WaitForNoInAnalysis();
            //UIMapScanCompletedWindow.ClickYesButton();
            //InputCamCredentials(TestContext);
            useMain.WaitForValkyrieResults(TestContext);
            UIHelper.ClickRow(endpoint);
            useMain.AssertPanelCountersData(0, 1, 1, TestContext);
            UIHelper.WaitForCellContent(ResultsScreen.ValkyrieAnalysis, unknown, Settings.ResultsValkyrie.Default.VerdictCellNumber, ValkyrieVerdict.NoThreatFound.ToUi(),
                                                                                                                TestContext, Settings.WaitingTime.Default.NoThreatFoundVerdictWaitTime);
            useMain.AssertPanelCountersData(1, 1, 0, TestContext);
        }
       
        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void CheckAllScanResultsTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            int ResolveEndpointTime = Settings.WaitingTime.Default.EndpointResolvingWaitTime;            

            // Offline endpoint
            IPAddress offlineIp = SysHelper.FindOfflineIp();            
            Computer offlineComputer = new Computer(offlineIp);

            StartEndpointScan(offlineComputer, ScanType.Quick, TestContext);            
            UIHelper.WaitForControlIsEnabled(SearchField(), ResolveEndpointTime);
            UIHelper.WaitForScanStatus(offlineComputer, ScanStatus.NotAccessible, TestContext);

            UIMapResultsWindow.ClickNewCustomScanButton();            
            UIMapStartScanWizard.ClickNetworkAddressButton();            

            // Failed scan
            StartEndpointScan(ScanType.Quick, DataHelper.Username + "error", DataHelper.Password, TestContext, endpoint);
            UIHelper.WaitForControlIsEnabled(SearchField(), ResolveEndpointTime);
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Failed, TestContext, ResolveEndpointTime);

            UIMapResultsWindow.ClickNewCustomScanButton();
            UIMapStartScanWizard.ClickNetworkAddressButton();            

            // Completed scan
            StartEndpointScan(endpoint, ScanType.Quick, TestContext);
            WaitForScanCompleted(ScanType.Quick);
            UIHelper.WaitForControlIsEnabled(SearchField(), Settings.WaitingTime.Default.QuickScanWaitTime);            
            UIHelper.WaitForScanStatus(endpoint, ScanStatus.Completed, TestContext);
        }

        [TestMethod, Timeout(DataHelper.LongTestTimeout)]
        public void ScanningWizardNavigationNetworkAddressesTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);

            UIHelper.WaitForControlIsClickable(RemoveAllButton());
            UIMapNetworkAddresses.ClickRemoveAllButton();

            // Network Addresses step
            UIMapNetworkAddresses.InputNetworkAddressParams.UINetworkAddressTextBoEditText = endpoint.Name;
            UIMapNetworkAddresses.InputNetworkAddress();
            UIMapNetworkAddresses.ClickAddButton();
            UIMapStartScanWizard.ClickBackButton();
            TestContext.WriteLine("Back to Start Scan Wizard...");
            UIMapStartScanWizard.ClickNetworkAddressButton();
            TestContext.WriteLine("Returned to Network Addresses step...");
            VerifyAdressListContainsHost(endpoint.Name);           
            UIMapStartScanWizard.ClickNextButton();
            // Scan Credentials step
            UIHelper.WaitForControl(QuickScanButton());
            InputScanLogin(DataHelper.Username);
            InputPassword(DataHelper.Password);
            UIMapStartScanWizard.ClickBackButton();
            TestContext.WriteLine("Back to Network Addresses step...");
            UIHelper.WaitForControl(NextButton());
            UIMapStartScanWizard.ClickNextButton();
            TestContext.WriteLine("Returned to Scan Credentials step...");
            UIHelper.WaitForControl(QuickScanButton());
            UIMapNetworkAddresses.AssertLoginTextBoxTextExpectedValues.UILoginWatermarkTextBoEditText = DataHelper.Username;
            UIMapNetworkAddresses.AssertLoginTextBoxText();
            TestContext.WriteLine(String.Format("Login '{0}' is stored in the respective text box.", DataHelper.Username));
            UIMapStartScanWizard.ClickQuickScanButton();
            TestContext.WriteLine("Quick Scan button clicked...");
            WaitForScanCompleted(ScanType.Quick);            
            UIMapResultsWindow.ClickNewCustomScanButton();
            UIHelper.WaitForControlIsClickable(NetworkAddressButton());
            UIMapStartScanWizard.ClickNetworkAddressButton();
            // Network Addresses step after scan
            TestContext.WriteLine("Returned to Network Addresses step after scan...");
            VerifyAdressListContainsHost(endpoint.Name);
            UIMapStartScanWizard.ClickNextButton();
            // Scan Credentials step after scan
            TestContext.WriteLine("Returned to Scan Credentials step after scan...");
            UIHelper.WaitForControl(QuickScanButton());
            UIMapNetworkAddresses.AssertLoginTextBoxTextExpectedValues.UILoginWatermarkTextBoEditText = DataHelper.Username;
            UIMapNetworkAddresses.AssertLoginTextBoxText();
            TestContext.WriteLine(String.Format("Login '{0}' is stored in the respective text box.", DataHelper.Username));
            UIMapStartScanWizard.ClickQuickScanButton();
            TestContext.WriteLine("Quick Scan button clicked...");
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, TestContext);
        }
        
        [TestMethod]
        public void InvalidInputHandlingInNetworkAddressesTest()
        {
            Computer endpoint = DataHelper.GetEndpoint();
            // Network address            
            UIHelper.WaitForControlIsClickable(RemoveAllButton());
            UIMapNetworkAddresses.ClickRemoveAllButton();
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyNetworkAddress, TestContext);
            UIMapNetworkAddresses.InputNetworkAddressParams.UINetworkAddressTextBoEditText = endpoint.Name;
            UIMapNetworkAddresses.InputNetworkAddress();
            UIMapStartScanWizard.ClickNextButton();
            AssertErrorText(Settings.Errors.Default.SpecifyNetworkAddress, TestContext);
            UIMapNetworkAddresses.ClickAddButton();
            UIMapStartScanWizard.ClickNextButton();           

            // Login
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyLogin, TestContext);
            UIMapNetworkAddresses.InputLoginParams.UILoginWatermarkTextBoEditSendKeys = DataHelper.Username;
            UIMapNetworkAddresses.InputLogin();

            // Password
            UIMapStartScanWizard.ClickQuickScanButton();
            AssertErrorText(Settings.Errors.Default.SpecifyPassword, TestContext);
        }
        #endregion
    }
}
