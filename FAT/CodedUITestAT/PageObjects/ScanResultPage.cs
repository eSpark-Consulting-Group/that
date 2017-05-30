using CodedUITestFAT.BusinessObjects.Reports;
using CodedUITestFAT.Helpers;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UIAssert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace CodedUITestFAT.BusinessObjects.Results
{
    class ScanResultPage : CodedUITestCommon
    {
        private enum collumns { Name, Size, Verdict }

        private WpfPane mainFrame;
        private Grid grid;
        //private WpfCustom screenControl;

        #region Buttons
        WpfButton NewCustomScanBtn;
        WpfButton DetailedScanResultBtn;
        WpfButton StartAutoDiscoveryBtn;


        #endregion

        internal ScanResultPage()
        {
            this.mainFrame = UIMapAggregatedPanel.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane;            
            this.grid = new Grid(mainFrame);            
            NewCustomScanBtn = UIMapResultsWindow.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIPleaseWaitProgressBar.UINewCustomScanButtonButton;
            DetailedScanResultBtn = UIMapResultsWindow.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIPleaseWaitProgressBar.UIDetailedScanResultsBButton;
            StartAutoDiscoveryBtn = UIMapDiscovery.UIForensicAnalysisToolWindow.UIViewsMainViewxamlPane.UIPleaseWaitProgressBar.UIStartAutoDiscoveryBuButton;
        }

        internal ScanResultPage ExpandRow(int row)
        {
            var control = grid.GetRowControlByID(row);
            if (ExpandRow(control))
            {
                return this;
            }
            //throw new AssertFailedException($"Couldn't find row by id: {row}");
            throw new AssertFailedException(String.Format("Couldn't find row by id: {0}", row));
        }

        internal ScanResultPage ExpandRowWith(IRow rowName)
        {
            var control = grid.GetRowControlByName(rowName);
            if (ExpandRow(control))
            {
                return this;
            }
            //throw new AssertFailedException($"Couldn't find row by name: {rowName.RowName}");
            throw new AssertFailedException(String.Format("Couldn't find row by name: {0}", rowName.RowName));
        }

        private bool ExpandRow(WpfControl row)
        {
            var expanderControl = new WpfCheckBox(row);
            expanderControl.SearchProperties[WpfControl.PropertyNames.AutomationId] = IDs.rowExpanderId;
            expanderControl.WindowTitles.Add(IDs.mainWindowTitle);

            Point point;
            if (expanderControl.TryGetClickablePoint(out point))
            {
                Mouse.Click(expanderControl, point);
                return true;
            }
            return false;
        }

        internal ScanResultPage CheckThatTileVisible(TileType tile)
        {
            WpfText control = GetResultTileTextControl(tile);
            UIAssert.IsTrue(control.Exists(), string.Format("Tile [{0}] is absent on the screen", Enum.GetName(typeof(TileType), tile)));
            return this;
        }

        internal ScanResultPage CheckThatTileNotVisible(TileType tile)
        {
            WpfText control = GetResultTileTextControl(tile);
            UIAssert.IsTrue(!control.Exists(), string.Format("Tile [{0}] is present on the screen", Enum.GetName(typeof(TileType), tile)));
            return this;
        }

        internal ScanResultPage CheckThatVerdictFor(Threat threatFile, string isExpectedVerdict)
        {
            var control = grid.GetRowControlByName(threatFile);
            string verdict = grid.GetCellText(control, (int)collumns.Verdict);
            UIAssert.AreEqual(isExpectedVerdict, verdict, true, String.Format("Expected verdict: [{0}], but actual is: [{1}]", isExpectedVerdict, verdict));
            //UIAssert.AreEqual(isExpectedVerdict, verdict, true, $"Expected verdict: {isExpectedVerdict}, but actual is {verdict}");
            return this;
        }

        internal void StartNewCustomScan()
        {
            if (!ClickBtn(NewCustomScanBtn))
                throw new AssertFailedException("Couldn't click on 'New Custom Scan' button");

        }

        internal void OpenDetailedScanResults()
        {
            if (!ClickBtn(DetailedScanResultBtn))
                throw new AssertFailedException("Couldn't click on 'Detailed Scan Result' button");

        }

        internal void StartAutoDiscovery()
        {
            if (!ClickBtn(StartAutoDiscoveryBtn))
                throw new AssertFailedException("Couldn't click on 'Start Auto Discovery' button");

        }

        private bool ClickBtn(WpfButton button) {            
            if (button == null)
                throw new ArgumentException("Button control could not be null");
            Point point;
            if (!button.TryGetClickablePoint(out point))
                return false;
            Mouse.Click(button, point);
            return true;
        }

        
    }
}
