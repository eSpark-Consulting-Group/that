using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;


namespace CodedUITestFAT.BusinessObjects.Results
{
    class PreviousScansPage : CodedUITestCommon
    {
        private enum collumns {Summary, Date, Target, Type, Link}

        private Grid grid;
        private WpfCustom screenControl;

        internal PreviousScansPage()
        {
            this.screenControl = UIMapResultsWindow.UIForensicAnalysisToolWindow.UIViewsScanPreviousScaPane.UIPreviousScansScreenCustom;
            this.grid = new Grid(screenControl);
        }

        internal void ClickRow(int row) {
            Mouse.Click(grid.GetCellUiControl(row, 0));
        }

        internal bool OpenDetailedResult(int row){
            return grid.ClickCell(row, (int)collumns.Link);
        }

        internal DateTime ScanDate(int row)
        {
            string rawDate = grid.GetCellText(row, (int)collumns.Date);
            return DateTime.Now;
        }
        internal string ScanDateString(int row)
        {
            string rawDate = grid.GetCellText(row, (int)collumns.Date);
            return rawDate;
        }
    }
}
