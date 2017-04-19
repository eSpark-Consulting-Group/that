using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestAT.UIMaps.UIMapEulaClasses;

namespace CodedUITestFAT
{
    /// <summary> Tests for EULA screen. </summary>
    [CodedUITest]
    public sealed class CodedUITestEula : CodedUITestCommon
    {
        public CodedUITestEula()
        {
        }

        #region UIMap properties

        /// <summary> UIMap for EULA screen, UIMapEula.uitest. </summary>
        public UIMapEula UIMapEula
        {
            get
            {
                if ((this.mapEula == null))
                {
                    this.mapEula = new UIMapEula();
                }

                return this.mapEula;
            }
        }
        private UIMapEula mapEula;

        #endregion

        #region Initialize and cleanup

        [TestInitialize]
        public void ATTestInitialize()
        {            
            SysHelper.StartProduct(DataHelper.GetEndpoint());
        }

        [TestCleanup]
        public void ATTestCleanup()
        {
            SysHelper.StopProduct();
        }

        #endregion

        #region Control methods

        internal WpfButton IAcceptButton()
        {
            return UIMapEula.UIForensicAnalysisToolWindow.UIIACCEPTButton;
        }

        private WpfControl EulaDocument()
        {
            return UIMapEula.UIForensicAnalysisToolWindow.UIEulaDocumentDocument;
        }

        #endregion

        #region Test methods
        
        [TestMethod]
        public void VerifyEulaDefaultsTest()
        {
            CodedUITestMain useMain = new CodedUITestMain();
            CodedUITestHelp useHelp = new CodedUITestHelp();
            Computer endpoint = DataHelper.GetEndpoint();
                        
            UIMapEula.ClickCancelButton();
            if (!useMain.MainWindow().Exists) TestContext.WriteLine("The main application window is closed after clicking Cancel button.");
            else Assert.Fail("The main application window is not closed after clicking Cancel button!");

            SysHelper.StartProduct(endpoint);
            UIMapEula.ClickIAcceptButton();
            if (useHelp.HelpButton().IsClickable()) TestContext.WriteLine("The main application window is available after clicking I Accept button.");
            else Assert.Fail("The main application window is not available after clicking I Accept button!");

            UIMapMain.ClickCloseButton();
            if (YesDialogButton().IsClickable()) UIMapStopScanWindow.ClickYesButton();
            SysHelper.StartProduct(endpoint);
            UIMapEula.ClickLicenseAgreementLink();
            UIMapEula.AssertEulaDocument();
            TestContext.WriteLine("EULA document is shown after clicking License Agreement link.");

            UIMapEula.ClickCancelButton();
            if (!useMain.MainWindow().Exists) TestContext.WriteLine("The main application window is closed after clicking Cancel button.");
            else Assert.Fail("The main application window is not closed after clicking Cancel button!");

            SysHelper.StartProduct(endpoint);
            UIMapEula.ClickLicenseAgreementLink();
            UIMapEula.ClickIAcceptButton();
            if (useHelp.HelpButton().IsClickable()) TestContext.WriteLine("The main application window is available after clicking I Accept button.");
            else Assert.Fail("The main application window is not available after clicking I Accept button!");
        }

        #endregion
    }
}
