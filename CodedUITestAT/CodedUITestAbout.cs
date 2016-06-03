using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestAT.UIMaps.UIMapAboutClasses;

namespace CodedUITestAT
{
    /// <summary> Tests for About screen. </summary>
    [CodedUITest]
    public sealed class CodedUITestAbout
    {
        public CodedUITestAbout()
        {
        }

        #region UIMap properties

        /// <summary> UIMap for About screen, UIMapAbout.uitest. </summary>
        public UIMapAbout UIMapAbout
        {
            get
            {
                if ((this.mapAbout == null))
                {
                    this.mapAbout = new UIMapAbout();
                }

                return this.mapAbout;
            }
        }
        private UIMapAbout mapAbout;

        #endregion

        #region Initialize and cleanup

        [TestInitialize]
        public void ATTestInitialize()
        {
            SysHelper.StartApplication(DataHelper.GetEndpoint());

            CodedUITestEula useEula = new CodedUITestEula();
            useEula.UIMapEula.CheckIAcceptTermsOfThisAgreementCheckBox();
            useEula.UIMapEula.ClickContinueButton();

            UIHelper.WaitForControl(AboutButton());
            UIHelper.WaitForControlIsEnabled(AboutButton());
        }

        [TestCleanup]
        public void ATTestCleanup()
        {
            SysHelper.StopApplication();
        }

        #endregion

        #region Control methods

        private WpfButton AboutButton()
        {
            return UIMapAbout.UIUnknownFileHunterWindow.UIViewsMainViewxamlPane.UIRadToolBarToolBar.UISystemWindowsControlButton;
        }
        
        #endregion

        #region Test methods

        [TestMethod]
        public void VerifyAboutWindowContentTest()
        {
            string productName = Settings.General.Default.ATApplicationName;
            string productVersion = SysHelper.GetProductVersion();
            string copyrightText = "© " + String.Format(Settings.General.Default.CopyrightText, DateTime.Now.Year.ToString());

            UIMapAbout.ClickAboutButton();

            // Checks the product name
            UIMapAbout.AssertProductNameValueTextBlockTextExpectedValues.UIUnknownFileHunterTextDisplayText = productName;
            UIMapAbout.AssertProductNameValueTextBlockText();
            TestContext.WriteLine(String.Format("The product name is correct: '{0}'.", productName));
            
            // Checks the product version
            UIMapAbout.AssertProductVersionValueTextBlockTextExpectedValues.UIItem14205242TextDisplayText = productVersion;
            UIMapAbout.AssertProductVersionValueTextBlockText();
            TestContext.WriteLine(String.Format("The product version is correct: '{0}'.", productVersion));
            
            // Checks the product copyright
            UIMapAbout.AssertCopyrightTextBlockTextExpectedValues.UIItem2016ComodoSecuriTextDisplayText = copyrightText;
            UIMapAbout.AssertCopyrightTextBlockText();
            TestContext.WriteLine(String.Format("The product copyright text is correct: '{0}'.", copyrightText));
            
            // Checks Close button
            UIMapAbout.ClickCloseButton();
            UIHelper.WaitForControlIsEnabled(AboutButton());
            UIMapAbout.AssertAboutWindowExpectedValues.UIWpfWindowExists = false;
            UIMapAbout.AssertAboutWindow();            
            TestContext.WriteLine("About window is successfully closed using Close button.");
            
            // Checks Close Cross button
            UIMapAbout.ClickAboutButton();
            UIMapAbout.ClickCloseCrossButton();
            UIHelper.WaitForControlIsEnabled(AboutButton());
            UIMapAbout.AssertAboutWindow();
            TestContext.WriteLine("About window is successfully closed using Close Cross button.");

            UIMapAbout.ClickAboutButton();
            UIMapAbout.AssertAboutWindowExpectedValues.UIWpfWindowExists = true;
            UIMapAbout.AssertAboutWindow();
            TestContext.WriteLine("About window can be opened again.");
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
