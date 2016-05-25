using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestAT.UIMaps.UIMapEulaClasses;

namespace CodedUITestAT
{
    /// <summary> Tests for EULA screen. </summary>
    [CodedUITest]
    public sealed class CodedUITestEula
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
            SysHelper.StartApplication();
        }

        [TestCleanup]
        public void ATTestCleanup()
        {
            SysHelper.StopApplication();
        }

        #endregion

        #region Control methods

        internal WpfCheckBox IAcceptTermsOfThisAgreementCheckBox()
        {
            return UIMapEula.UICOMODOUnknownFileHunWindow.UIIacceptthetermsofthiCheckBox;
        }

        private WpfButton ContinueButton()
        {
            return UIMapEula.UICOMODOUnknownFileHunWindow.UIContinueButton;
        }

        #endregion

        #region Test methods
        
        [TestMethod]
        public void VerifyEulaDefaultsTest()
        {
            if (!IAcceptTermsOfThisAgreementCheckBox().Checked) TestContext.WriteLine("'I accept the terms of this agreement' checkbox is unchecked by default.\n");
            else Assert.Fail("'I accept the terms of this agreement' checkbox is checked by default!\n");

            if (!ContinueButton().Enabled) TestContext.WriteLine("Continue button is disabled by default.\n");
            else Assert.Fail("Continue button is enabled by default!\n");

            UIMapEula.AssertEulaText();
            TestContext.WriteLine("EULA text is correct.\n");

            UIMapEula.CheckIAcceptTermsOfThisAgreementCheckBox();
            UIMapEula.ClickContinueButton();

            CodedUITestMain useMain = new CodedUITestMain();
            UIHelper.WaitForControlIsEnabled(useMain.StartScanningButton());
            TestContext.WriteLine("Main screen appeared.\n");
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
