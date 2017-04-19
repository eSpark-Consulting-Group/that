using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestAT.UIMaps.UIMapHelpClasses;

namespace CodedUITestFAT
{
    /// <summary> Tests for Help and About screens. </summary>
    [CodedUITest]
    public sealed class CodedUITestHelp : CodedUITestCommon
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
            SysHelper.StartProduct(DataHelper.GetEndpoint());

            CodedUITestEula useEula = new CodedUITestEula();            
            useEula.UIMapEula.ClickIAcceptButton();

            UIHelper.WaitForControl(HelpButton());
            UIHelper.WaitForControlIsEnabled(HelpButton());
        }

        [TestCleanup]
        public void ATTestCleanup()
        {
            SysHelper.StopProduct();
        }

        #endregion

        #region Control methods

        internal WpfButton HelpButton()
        {
            return UIMapHelp.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIMainToolBarToolBar.UIHelpButton;
        }

        #endregion

        #region
        /// <summary> Asserts whether 'Agent requirements' window exists. </summary>
        /// <param name="exists">Set an expected result.</param>
        private void AssertAgentWindow(bool exists)
        {
           UIMapHelp.UIUnknownFileHunterWindow1.UIWpfWindow2.SearchProperties.Clear();           
           UIMapHelp.UIUnknownFileHunterWindow1.UIWpfWindow2.SearchProperties[WpfWindow.PropertyNames.Name] = "AgentRequirementsWindow";
           UIMapHelp.AssertAgentRequirementsWindowExpectedValues.UIWpfWindow2Exists = exists;
           UIMapHelp.AssertAgentRequirementsWindow();
        }

        #endregion

        #region Test methods

        [TestMethod]
        public void VerifyAboutWindowContentTest()
        {
            string productName = Settings.General.Default.FATApplicationName;
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
            UIMapHelp.ClickCloseAboutButton();
            UIHelper.WaitForControlIsEnabled(HelpButton());
            UIMapHelp.AssertAboutWindowExpectedValues.UIWpfWindowExists = false;
            UIMapHelp.AssertAboutWindow();
            TestContext.WriteLine("About window is successfully closed using Close button.");
            
            // Checks Close Cross button
            UIMapHelp.ClickHelpButton();
            UIMapHelp.SelectAboutMenuItem();
            UIMapHelp.ClickCloseAboutCrossButton();
            UIHelper.WaitForControlIsEnabled(HelpButton());
            UIMapHelp.AssertAboutWindow();
            TestContext.WriteLine("About window is successfully closed using Close Cross button.");

            UIMapHelp.ClickHelpButton();
            UIMapHelp.SelectAboutMenuItem();
            UIMapHelp.AssertAboutWindowExpectedValues.UIWpfWindowExists = true;
            UIMapHelp.AssertAboutWindow();
            TestContext.WriteLine("About window can be opened again.");
        }

        [TestMethod]
        public void VerifyAgentRequirementsWindowContentTest()
        {
            UIMapHelp.ClickHelpButton();
            UIMapHelp.SelectAgentRequirementsMenuItem();
                        
            UIMapHelp.AssertTurnedOnTextBlockText();
            TestContext.WriteLine(String.Format("'{0}' requirement is shown correctly.", UIMapHelp.AssertTurnedOnTextBlockTextExpectedValues.UICheckwhetheratargetcTextDisplayText));

            UIMapHelp.AssertFirewallOffTextBlockText();
            TestContext.WriteLine(String.Format("'{0}' requirement is shown correctly.", UIMapHelp.AssertFirewallOffTextBlockTextExpectedValues.UICheckwhetherfirewallTextDisplayText));

            UIMapHelp.AssertIpAddressTextBlockText();
            TestContext.WriteLine(String.Format("'{0}' requirement is shown correctly.", UIMapHelp.AssertIpAddressTextBlockTextExpectedValues.UICheckwhetherIPaddresTextDisplayText));

            UIMapHelp.AssertEnableSharingTextBlockText();
            TestContext.WriteLine(String.Format("'{0}' requirement is shown correctly.", UIMapHelp.AssertEnableSharingTextBlockTextExpectedValues.UIIntheNetworkNeighbouTextDisplayText));

            UIMapHelp.AssertPasswordNotBlankTextBlockText();
            TestContext.WriteLine(String.Format("'{0}' requirement is shown correctly.", UIMapHelp.AssertPasswordNotBlankTextBlockTextExpectedValues.UIMakesureAdministratoTextDisplayText));

            UIMapHelp.AssertForceguestTextBlockText();
            TestContext.WriteLine(String.Format("'{0}' requirement is shown correctly.", UIMapHelp.AssertForceguestTextBlockTextExpectedValues.UICheckwhetherForcegueTextDisplayText));

            UIMapHelp.AssertLocalAccountTokenTextBlockText();
            TestContext.WriteLine(String.Format("'{0}' requirement is shown correctly.", UIMapHelp.AssertLocalAccountTokenTextBlockTextExpectedValues.UIIftheaccountisnotabuTextDisplayText));

            // Close button
            UIMapHelp.ClickCloseAgentRequirementsButton();
            UIHelper.WaitForControlIsEnabled(HelpButton());
            AssertAgentWindow(false);
            TestContext.WriteLine("'Agent Requirements' window is successfully closed using Close button.");

            // Close cross button
            UIMapHelp.ClickHelpButton();
            UIMapHelp.SelectAgentRequirementsMenuItem();
            UIMapHelp.ClickCloseAgentRequirementsCrossButton();
            UIHelper.WaitForControlIsEnabled(HelpButton());
            AssertAgentWindow(false);
            TestContext.WriteLine("'Agent Requirements' window is successfully closed using Close cross button.");

            UIMapHelp.ClickHelpButton();
            UIMapHelp.SelectAgentRequirementsMenuItem();
            AssertAgentWindow(true);
            TestContext.WriteLine("'Agent Requirements' window can be opened again.");
        }

        #endregion        
    }
}
