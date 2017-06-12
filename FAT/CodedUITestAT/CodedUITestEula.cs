using CodedUITestFAT.Helpers;
using CodedUITestFAT.UIMaps.UIMapEulaClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;


namespace CodedUITestFAT
{
    /// <summary> Tests for EULA screen. </summary>
    [CodedUITest]
    public sealed class CodedUITestEula : CodedUITestCommon
    {
        public CodedUITestEula()
        {
        }       

        #region Initialize and cleanup

        [TestInitialize]
        public void ATTestInitialize()
        {
            app = app.Start();
            WaitForAppIsRunning(app);            
        }

        [TestCleanup]
        public void ATTestCleanup()
        {
            SysHelper.StopProduct();
        }

        #endregion

        #region Control methods              

        private WpfControl EulaDocument()
        {
            return UIMapEula.UIForensicAnalysisToolWindow.UIEulaDocumentDocument;
        }

        #endregion

        #region Test methods
        
        [TestMethod]
        [TestCategory("Smoke"), TestCategory("UiDefaults")]
        public void VerifyEulaDefaultsTest()
        {
            //CodedUITestMain useMain = new CodedUITestMain();
            CodedUITestHelp useHelp = new CodedUITestHelp();            
                        
            UIMapEula.ClickCancelButton();
            if (!MainWindow().Exists) TestContext.WriteLine("The main application window is closed after clicking Cancel button.");
            else Assert.Fail("The main application window is not closed after clicking Cancel button!");

            SysHelper.StartProduct();
            UIMapEula.ClickIAcceptButton();
            if (useHelp.HelpButton().IsClickable()) TestContext.WriteLine("The main application window is available after clicking I Accept button.");
            else Assert.Fail("The main application window is not available after clicking I Accept button!");

            UIMapMain.ClickCloseButton();
            if (YesDialogButton().IsClickable()) UIMapStopScanWindow.ClickYesButton();
            SysHelper.StartProduct();
            UIMapEula.ClickLicenseAgreementLink();
            UIMapEula.AssertEulaDocument();
            TestContext.WriteLine("EULA document is shown after clicking License Agreement link.");

            UIMapEula.ClickCancelButton();
            if (!MainWindow().Exists) TestContext.WriteLine("The main application window is closed after clicking Cancel button.");
            else Assert.Fail("The main application window is not closed after clicking Cancel button!");

            SysHelper.StartProduct();
            UIMapEula.ClickLicenseAgreementLink();
            UIMapEula.ClickIAcceptButton();
            if (useHelp.HelpButton().IsClickable()) TestContext.WriteLine("The main application window is available after clicking I Accept button.");
            else Assert.Fail("The main application window is not available after clicking I Accept button!");
        }

        #endregion
    }
}
