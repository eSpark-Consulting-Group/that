using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestAT.UIMaps.UIMapMainClasses;
using CodedUITestAT.UIMaps.StartScanningWizard.UIMapStartScanningWizardClasses;
using CodedUITestAT.UIMaps.StartScanningWizard.UIMapNetworkAddressesClasses;
using CodedUITestAT.UIMaps.UIMapScanCompletedWindowClasses;
using CodedUITestAT.UIMaps.UIMapCamCredentialsWindowClasses;
using CodedUITestAT.UIMaps.UIMapResultsWindowClasses;

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

        #endregion
        
        #region Initialize and cleanup

        [TestInitialize]
        public void ATTestInitialize()
        {
            SysHelper.StartProcess(DataHelper.GetWorkingFolder(), DataHelper.ProductExeName, String.Empty);
            UIHelper.WaitForControlIsEnabled(IAcceptTermsOfThisAgreementCheckBox());
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

        private WpfCheckBox IAcceptTermsOfThisAgreementCheckBox()
        {
            return UIMapMain.UICOMODOUnknownFileHunWindow.UIIacceptthetermsofthiCheckBox;
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

        #endregion

        #region Test methods

        [TestMethod]
        public void QuickScanTest()
        {
            string endpointName = DataHelper.GetEndpointName();
            
            UIMapMain.CheckIAcceptTermsOfThisAgreementCheckBox();
            UIMapMain.ClickContinueButton();

            UIHelper.WaitForControlIsEnabled(StartScanningButton());
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

            // Gets the number of Unknown files found at Scan Completed window
            string unknownFilesSearchWord = Settings.ScanCompleted.Default.UnknownFiles;
            string unknownFilesPattern = @"\d{1,}\s{1,}" + unknownFilesSearchWord;
            string match = Regex.Match(ScanStatisticsTextBlock().DisplayText, unknownFilesPattern).ToString().Replace(unknownFilesSearchWord, String.Empty);
            int expectedUnknownFilesNumber;
            if (Int32.TryParse(match, out expectedUnknownFilesNumber))
                    TestContext.WriteLine("Unknown files number successfully found at Scan Completed window.");
            else Assert.Fail("Unknown files number is not found at Scan Completed window!");
            UIMapScanCompletedWindow.ClickOkButton();
            
            if (expectedUnknownFilesNumber == 0) UIMapMain.ClickAnalyseWithValkyrieButton();

            UIHelper.WaitForControlIsEnabled(CamLoginTextBox());
            TestContext.WriteLine("CAM Credentials window appeared...\n");
            UIMapCamCredentialsWindow.InputLoginParams.UILoginWatermarkTextBoEditSendKeys = Settings.General.Default.CamLogin;
            UIMapCamCredentialsWindow.InputLogin();
            UIMapCamCredentialsWindow.InputPasswordParams.UIPasswordBoxEditSendKeys = Playback.EncryptText(Settings.General.Default.CamPassword);
            UIMapCamCredentialsWindow.InputPassword();
            UIMapCamCredentialsWindow.ClickOkButton();

            // Gets the number of rows in 'Valkyrie analysis results' and compares it with the number of Unknown files found
            UIHelper.WaitForControl(ValkyrieResultsTree());
            TestContext.WriteLine("'Valkyrie analysis results' screen appeared...\n");
            var valkyrieResultsRows = ValkyrieResultsTree().GetTreeRows();
            int valkyrieResultsRowsNumber = valkyrieResultsRows.Count;
            if (valkyrieResultsRowsNumber == expectedUnknownFilesNumber)
                TestContext.WriteLine(String.Format("The number of Unknown files found {0} and the number of files at 'Valkyrie analysis results' screen {1} coincide.\n", expectedUnknownFilesNumber, valkyrieResultsRowsNumber));
            else Assert.Fail(String.Format("The number of Unknown files found {0} and the number of files at 'Valkyrie analysis results' screen {1} do not coincide!", expectedUnknownFilesNumber, valkyrieResultsRowsNumber));

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
