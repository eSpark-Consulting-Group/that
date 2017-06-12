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
        /// <summary>
        /// Represents a previous scans page
        /// </summary>
        internal PreviousScansPage()
        {
            this.screenControl = UIMapResultsWindow.UIForensicAnalysisToolWindow.UIViewsScanPreviousScaPane.UIPreviousScansScreenCustom;
            this.grid = new Grid(screenControl);
        }

        /// <summary> Clicks the row with specified row id. </summary>
        /// <param name="rowId">Row id. Part of Automaition id: Row_N</param>        
        
        internal void ClickRow(int rowId) {
            Mouse.Click(grid.GetCellUiControl(rowId, 0));
        }

        /// <summary>
        /// Clicks on the link in the specific row.
        /// </summary>
        /// <param name="rowId">Row id. Part of Automaition id: Row_N</param>
        /// <returns>Returns true if click was successfull</returns>
        internal bool OpenDetailedResult(int rowID){
            return grid.ClickCell(rowID, (int)collumns.Link);
        }
        
    }
}
