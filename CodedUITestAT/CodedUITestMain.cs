using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestAT.UIMaps.UIMapMainClasses;
using CodedUITestAT.UIMaps.StartScanningWizard.UIMapStartScanningWizardClasses;
using CodedUITestAT.UIMaps.StartScanningWizard.UIMapNetworkAddressesClasses;
using CodedUITestAT.UIMaps.UIMapScanCompletedWindowClasses;

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

        #region Start Scanning Wizard screen

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
            TestContext.WriteLine("Start Scanning Wizard appeared...");
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
            TestContext.WriteLine(String.Format("Quick Scan started at endpoint '{0}'...", endpointName));
            
            UIHelper.WaitForControl(ScanCompletedTextBlock(), Settings.WaitingTime.Default.QuickScanWaitTime);

            UIMapScanCompletedWindow.AssertScanCompletedTextBlockTextExpectedValues.UIScanCompletedDoyouwaTextDisplayText = Settings.ScanCompleted.Default.ScanCompletedText;
            UIMapScanCompletedWindow.AssertScanCompletedTextBlockText();
            TestContext.WriteLine(String.Format("Scan Completed window appered with expected text: '{0}'", Settings.ScanCompleted.Default.ScanCompletedText));
            UIMapScanCompletedWindow.ClickOkButton();
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
