using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestAT.UIMaps.UIMapMainClasses;
using CodedUITestAT.UIMaps.StartScanWizard.UIMapStartScanWizardClasses;
using CodedUITestAT.UIMaps.StartScanWizard.UIMapThisComputerClasses;

namespace CodedUITestAT
{
    /// <summary> Tests for This Computer branch. </summary>
    [CodedUITest]
    public sealed class CodedUITestThisComputer : CodedUITestCommon
    {
        public CodedUITestThisComputer()
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
        
        /// <summary> UIMap for This Computer branch, UIMapThisComputer.uitest. </summary>
        public UIMapThisComputer UIMapThisComputer
        {
            get
            {
                if ((this.mapThisComputer == null))
                {
                    this.mapThisComputer = new UIMapThisComputer();
                }

                return this.mapThisComputer;
            }
        }
        private UIMapThisComputer mapThisComputer;

        #endregion

        #region Initialize and cleanup

        [TestInitialize]
        public void ATTestInitialize()
        {
            SysHelper.StartApplication(DataHelper.GetEndpoint());

            CodedUITestEula useEula = new CodedUITestEula();
            CodedUITestMain useMain = new CodedUITestMain();
            useEula.UIMapEula.ClickIAcceptButton();

            UIHelper.WaitForControlIsEnabled(useMain.StartScanButton());
            UIMapMain.ClickStartScanButton();
            UIHelper.WaitForControl(ThisComputerButton());
            UIMapStartScanWizard.ClickThisComputerButton();
            UIHelper.WaitForControl(CustomScanButton());
        }

        [TestCleanup]
        public void ATTestCleanup()
        {
            Computer endpoint = DataHelper.GetEndpoint();

            SysHelper.StopApplication();
            SysHelper.DisinfectForQuickScan(endpoint, TestContext);
            SysHelper.StopScanner(endpoint);
        }

        #endregion

        #region Control methods       

        private WpfButton ThisComputerButton()
        {
            return UIMapStartScanWizard.UIUnknownFileHunterWindow1.UIScanningWizardWindowCustom.UILoadingProgressBar.UIScanThisComputerButton;
        }
        private WpfButton CustomScanButton()
        {
            return UIMapThisComputer.UIUnknownFileHunterWindow.UIScanningWizardWindowCustom.UIThisComputerScreenCustom.UICustomScanButton;
        }

        private WpfCheckBox ScanCriticalAreasCheckBox()
        {
            return UIMapThisComputer.UIUnknownFileHunterWindow.UIScanningWizardWindowCustom.UIThisComputerScreenCustom.UIScancriticalareasofcCheckBox;
        }

        #endregion

        #region Local helper methods       

        /// <summary> Locally copies a given threat to each given path. </summary>
        /// <param name="paths">Set a path.</param>
        /// <param name="threat">Set a threat.</param>
        private void CopyThreatTo(List<string> paths, Threat threat)
        {
            foreach (string path in paths)
            {
                string destination = String.Format(@"{0}\{1}", path, threat.Name);
                File.Copy(threat.Location, destination, true);
            }
        }

        /// <summary> Expands a given file browser tree item using its name in accordance with APTAT.Application.Models.FolderItem. </summary>
        /// <param name="treeItemName">Set a tree item unique name.</param>
        private void ExpandFileBrowserTreeItem(string treeItemName)
        {
            if (treeItemName.EndsWith(@"\")) treeItemName = treeItemName.Remove(treeItemName.Length - 1);
            UIMapThisComputer.UIUnknownFileHunterWindow.UIThisComputerScreenCustom.UIFileBrowserTreeTree.UICTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name] = treeItemName;
            UIMapThisComputer.ExpandFileBrowserTreeItem();
        }

        /// <summary> Checks a given file browser tree item using its name in accordance with APTAT.Application.Models.FolderItem. </summary>
        /// <param name="treeItemName">Set a tree item unique name.</param>
        private void CheckFileBrowserTreeItem(string treeItemName)
        {
            if (treeItemName.EndsWith(@"\")) treeItemName = treeItemName.Remove(treeItemName.Length - 1);
            UIMapThisComputer.UIUnknownFileHunterWindow.UIThisComputerScreenCustom.UIFileBrowserTreeTree.UICTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name] = treeItemName;
            UIMapThisComputer.CheckFileBrowserTreeItem();
        }

        /// <summary> Determines whether a given file browser tree item is checked using its name in accordance with APTAT.Application.Models.FolderItem. </summary>
        /// <param name="treeItemName">Set a tree item unique name.</param>
        private bool IsFileBrowserTreeItemChecked(string treeItemName)
        {
            if (treeItemName.EndsWith(@"\")) treeItemName = treeItemName.Remove(treeItemName.Length - 1);
            UIMapThisComputer.UIUnknownFileHunterWindow.UIThisComputerScreenCustom.UIFileBrowserTreeTree.UICTreeItem.SearchProperties[WpfTreeItem.PropertyNames.Name] = treeItemName;
            WpfCheckBox checkbox = UIMapThisComputer.UIUnknownFileHunterWindow.UIThisComputerScreenCustom.UIFileBrowserTreeTree.UICTreeItem.UICheckBoxElementCheckBox;
            return checkbox.Checked;
        }

        /// <summary> Splits a given path in a list of all its subpaths. </summary>
        /// <param name="path">Set a path.</param>
        /// <returns>A list of subpaths sorted by length in ascending order.</returns>
        private List<string> SplitPath(string path)
        {
            List<string> paths = new List<string>();
            string separator = @"\";

            do
            {
                paths.Add(path);
                path = path.Remove(path.LastIndexOf(separator));
                path = path.Remove(path.LastIndexOf(separator) + 1);
            } while (path.Contains(separator));

            paths = paths.OrderBy(p => p.Length).ToList();

            return paths;
        }

        #endregion

        #region Test methods

        [TestMethod]
        public void ScanDirectoriesHierarchyTest()
        {            
            string fullPath1 = String.Format(@"{0}1\1.1\1.2\1.3\1.4\", DataHelper.GetServerRoot());
            string fullPath2 = String.Format(@"{0}2\2.1\2.2\", DataHelper.GetServerRoot());
            var paths1 = SplitPath(fullPath1);
            var paths2 = SplitPath(fullPath2);
            int subfoldersNumber = 3;
            string[] additinalPaths = new string[subfoldersNumber];
            UnknownFile unknown = SysHelper.AddUnknownFileForScan(new Computer(), ScanType.Quick);
            
            // Creates folders
            for (int i = 0; i < subfoldersNumber; i++)
            {
                additinalPaths[i] = String.Format(@"{0}1.5.{1}\", fullPath1, i+1);
                if (!Directory.Exists(additinalPaths[i])) Directory.CreateDirectory(additinalPaths[i]);
                //File.Copy(unknown.Location, String.Format(@"{0}\{1}", additinalPaths[i], unknown.Name), true);
            }
            if (!Directory.Exists(fullPath2)) Directory.CreateDirectory(fullPath2);

            // Copies threats to all the prepared folders
            var pathsToAddThreat = new List<string>(paths1);
            pathsToAddThreat.AddRange(paths2);
            pathsToAddThreat.AddRange(additinalPaths);
            CopyThreatTo(pathsToAddThreat, unknown);

            UIMapThisComputer.ClickCustomScanButton();
            UIHelper.WaitForControl(ScanCriticalAreasCheckBox());
            
            // Expands the prepared trees and checks some items
            var itemsToExpand = new List<string>(paths1);
            var paths2cut = new List<string>(paths2);
            paths2cut.Remove(Path.GetPathRoot(fullPath2));
            itemsToExpand.AddRange(paths2cut);
            foreach (var path in itemsToExpand) ExpandFileBrowserTreeItem(path);            
            CheckFileBrowserTreeItem(additinalPaths[0]);
            CheckFileBrowserTreeItem(additinalPaths[2]);
            CheckFileBrowserTreeItem(paths2[2]);

            // Verifies, which items are actually checked
            var uncheckedItems = new List<string>(paths1);
            uncheckedItems.Add(additinalPaths[1]);
            uncheckedItems.Add(paths2[1]);
            foreach (var path in uncheckedItems)
            {
                if (!IsFileBrowserTreeItemChecked(path)) TestContext.WriteLine(String.Format(@"Tree item '{0}' is not checked as expected.", path));
                else Assert.Fail(String.Format(@"Tree item '{0}' is checked, not expected!", path));
            }

            var checkedItems = new List<string>();
            checkedItems.Add(additinalPaths[0]);
            checkedItems.Add(additinalPaths[2]);
            checkedItems.Add(paths2[2]);
            checkedItems.Add(paths2[3]);
            foreach (var path in checkedItems)
            {
                if (IsFileBrowserTreeItemChecked(path)) TestContext.WriteLine(String.Format(@"Tree item '{0}' is checked as expected.", path));
                else Assert.Fail(String.Format(@"Tree item '{0}' is not checked, not expected!", path));
            }

            UIMapThisComputer.CheckScanCriticalAreasCheckBox();
            UIMapStartScanWizard.ClickNextButton();
            WaitForScanCompleted(ScanType.Quick);
            UIMapScanCompletedWindow.ClickNoButton();

            // Verifies the expected and unexpected files at 'Scan results' screen            
            foreach (var path in checkedItems)
            {
                var expectedFile = new UnknownFile(String.Format("{0}{1}", path, unknown.Name));
                UIHelper.FindThreat(expectedFile, ResultsScreen.ScanResults, TestContext);
            }            
            foreach (var path in uncheckedItems)
            {
                var unexpectedFile = new UnknownFile(String.Format("{0}{1}", path, unknown.Name));
                try
                {
                   UIHelper.FindThreat(unexpectedFile, ResultsScreen.ScanResults, TestContext);
                } catch (AssertFailedException)
                {
                  TestContext.WriteLine(String.Format(@"Row {0} not found at results screen as expected.", unexpectedFile.RowName));
                }
            }
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