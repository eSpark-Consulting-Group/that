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
            SysHelper.StartApplication(DataHelper.GetEndpoint());
        }

        [TestCleanup]
        public void ATTestCleanup()
        {
            SysHelper.StopApplication();
        }

        #endregion

        #region Control methods

        internal WpfButton IAcceptButton()
        {
            return UIMapEula.UIUnknownFileHunterWindow.UIViewsEULAViewxamlPane.UIEulaWindowCustom.UIIAcceptButton;
        }

        #endregion

        #region Test methods
        
        [TestMethod]
        public void VerifyEulaDefaultsTest()
        {
            UIMapEula.AssertEulaDocument();
            TestContext.WriteLine("EULA document is shown.");

            UIMapEula.ClickIAcceptButton();
            CodedUITestMain useMain = new CodedUITestMain();
            if (useMain.MainWindow().Exists) TestContext.WriteLine("Main application window is successfully opened after clicking 'I Accept' button.");
            else Assert.Fail("Main application window is not opened after clicking 'I Accept' button!");
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
