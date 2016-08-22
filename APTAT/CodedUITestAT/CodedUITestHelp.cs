using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestAT.UIMaps.UIMapHelpClasses;

namespace CodedUITestAT
{
    /// <summary> Tests for Help and About screens. </summary>
    [CodedUITest]
    public sealed class CodedUITestHelp
    {
        public CodedUITestHelp()
        {
        }

        #region UIMap properties

        /// <summary> UIMap for Help screen, UIMapHelp.uitest. </summary>
        public UIMapHelp UIMapHelp
        {
            get
            {
                if ((this.mapHelp == null))
                {
                    this.mapHelp = new UIMapHelp();
                }

                return this.mapHelp;
            }
        }
        private UIMapHelp mapHelp;

        #endregion

        #region Initialize and cleanup

        [TestInitialize]
        public void ATTestInitialize()
        {
            SysHelper.StartApplication(DataHelper.GetEndpoint());

            CodedUITestEula useEula = new CodedUITestEula();
            useEula.UIMapEula.CheckIAcceptTermsOfThisAgreementCheckBox();
            useEula.UIMapEula.ClickContinueButton();

            UIHelper.WaitForControl(HelpButton());
            UIHelper.WaitForControlIsEnabled(HelpButton());
        }

        [TestCleanup]
        public void ATTestCleanup()
        {
            SysHelper.StopApplication();
        }

        #endregion

        #region Control methods

        internal WpfButton HelpButton()
        {
            return UIMapHelp.UIUnknownFileHunterWindow.UIViewsMainViewxamlPane.UIRadToolBarToolBar.UIHelpButtonButton;
        }
        
        #endregion

        #region Test methods

        [TestMethod]
        public void VerifyAboutWindowContentTest()
        {
            string productName = Settings.General.Default.ATApplicationName;
            string productVersion = SysHelper.GetProductVersion();
            string copyrightText = "© " + String.Format(Settings.General.Default.CopyrightText, DateTime.Now.Year.ToString());

            UIMapHelp.ClickHelpButton();
            UIMapHelp.SelectAboutMenuItem();

            // Checks the product name
            UIMapHelp.AssertProductNameValueTextBlockTextExpectedValues.UIUnknownFileHunterTextDisplayText = productName;
            UIMapHelp.AssertProductNameValueTextBlockText();
            TestContext.WriteLine(String.Format("The product name is correct: '{0}'.", productName));
            
            // Checks the product version
            UIMapHelp.AssertProductVersionValueTextBlockTextExpectedValues.UIItem14205242TextDisplayText = productVersion;
            UIMapHelp.AssertProductVersionValueTextBlockText();
            TestContext.WriteLine(String.Format("The product version is correct: '{0}'.", productVersion));
            
            // Checks the product copyright
            UIMapHelp.AssertCopyrightTextBlockTextExpectedValues.UIItem2016ComodoSecuriTextDisplayText = copyrightText;
            UIMapHelp.AssertCopyrightTextBlockText();
            TestContext.WriteLine(String.Format("The product copyright text is correct: '{0}'.", copyrightText));
            
            // Checks Close button
            UIMapHelp.ClickCloseButton();
            UIHelper.WaitForControlIsEnabled(HelpButton());
            UIMapHelp.AssertAboutWindowExpectedValues.UIWpfWindowExists = false;
            UIMapHelp.AssertAboutWindow();
            TestContext.WriteLine("About window is successfully closed using Close button.");
            
            // Checks Close Cross button
            UIMapHelp.ClickHelpButton();
            UIMapHelp.SelectAboutMenuItem();
            UIMapHelp.ClickCloseCrossButton();
            UIHelper.WaitForControlIsEnabled(HelpButton());
            UIMapHelp.AssertAboutWindow();
            TestContext.WriteLine("About window is successfully closed using Close Cross button.");

            UIMapHelp.ClickHelpButton();
            UIMapHelp.SelectAboutMenuItem();
            UIMapHelp.AssertAboutWindowExpectedValues.UIWpfWindowExists = true;
            UIMapHelp.AssertAboutWindow();
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
