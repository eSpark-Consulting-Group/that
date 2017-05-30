using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using CodedUITestFAT.Helpers;

namespace CodedUITestFAT.BusinessObjects.Results
{
    internal class Grid
    {
        private WpfControl gridControl;

        private string rowMask = "Row_{0}";
        private string cellMask = "Cell_{0}_{1}";



        public Grid(WpfControl screenControl)
        {
            this.gridControl = new WpfControl(screenControl);
            this.gridControl.SearchProperties[WpfEdit.PropertyNames.AutomationId] = IDs.gridViewID;
            //this.gridControl.WindowTitles.Add(IDs.mainWindowTitle);
        }

        internal int RowCount
        {
            get
            {
                return gridControl.GetChildren().Count;
            }
        }

        internal bool ClickCell(int row, int collumn)
        {
            var desiredCollumn = GetCellUiControl(row, collumn);
            if (desiredCollumn == null)
                return false;
            Point point;
            if (desiredCollumn.TryGetClickablePoint(out point))
                return false;
            Mouse.Click(desiredCollumn, point);
            return true;
        }

        internal WpfControl GetRowControlByName(IRow row)
        {
            var rowName = row.RowName;
            var control = new WpfControl(gridControl);
            control.SearchProperties.Add(new PropertyExpression(WpfControl.PropertyNames.Name, rowName, PropertyExpressionOperator.Contains));
            return control;
        }

        internal WpfControl GetRowControlByID(int row)
        {
            var control = new WpfControl(gridControl);
            control.SearchProperties[WpfControl.PropertyNames.AutomationId] = String.Format(rowMask, row);
            return control;
        }

        internal string GetCellText(int row, int collumn)
        {
            var cellControl = GetCellUiControl(row, collumn);
            WpfText textBoxControl = new WpfText(cellControl);
            textBoxControl.SearchProperties[WpfText.PropertyNames.ClassName] = IDs.textBlockClassName;
            //textBoxControl.WindowTitles.Add(mainWindowTitle);
            return textBoxControl.DisplayText;
        }

        internal string GetCellText(WpfControl rowControl, int collumn)
        {
            //rowControl.DrawHighlight();
            string rowAutId = rowControl.GetProperty(WpfControl.PropertyNames.AutomationId) as string;
            int rowId;
            if (!int.TryParse(rowAutId.Substring(rowAutId.IndexOf('_') + 1), out rowId))
                throw new ArgumentException("Could not get id of provided row control");

            var cellControl = GetCellUiControl(rowId, collumn);
            //cellControl.DrawHighlight();
            WpfText textBoxControl = new WpfText(cellControl);
            textBoxControl.SearchProperties[WpfControl.PropertyNames.ClassName] = IDs.textBlockClassName;
            textBoxControl.WindowTitles.Add(IDs.mainWindowTitle);
            //textBoxControl.DrawHighlight();
            return textBoxControl.DisplayText;
        }
      
        internal WpfControl GetCellUiControl(int row, int collumn)
        {
            if (row < 0 | collumn < 0)
                throw new ArgumentException("Row index is out of range");

            WpfControl rowControl = new WpfControl(this.gridControl);
            rowControl.SearchProperties[WpfControl.PropertyNames.AutomationId] = String.Format(rowMask, row);
            rowControl.WindowTitles.Add(IDs.mainWindowTitle);

            WpfControl cellControl = new WpfControl(rowControl);
            cellControl.SearchProperties[WpfControl.PropertyNames.AutomationId] = String.Format(cellMask, row, collumn);
            cellControl.WindowTitles.Add(IDs.mainWindowTitle);

            return cellControl;
        }
        internal WpfControl GetControlBySearchProperty(string SearchProperty, string value)
        {
            if (string.IsNullOrEmpty(SearchProperty) | string.IsNullOrEmpty(value))
                throw new ArgumentException("SearchProperty and value should not be null or empty");

            WpfControl control = new WpfControl(this.gridControl);
            control.SearchProperties[SearchProperty] = value;
            //rowControl.WindowTitles.Add(mainWindowTitle);
            return control;
        }
    }
}
