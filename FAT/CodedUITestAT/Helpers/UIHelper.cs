using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace CodedUITestFAT
{
    /// <summary> Contains common methods for work with UI. </summary>
    internal static class UIHelper
    {
        /// <summary> Waits for a specific UI control appearance. </summary>
        /// <param name="control">Set a UI control.</param>
        /// <param name="waitForMilliseconds">Set the waiting time in milliseconds or keep default.</param>
        internal static void WaitForControl(UITestControl control, int waitForMilliseconds = DataHelper.DefaultWaitTime)
        {
            control.WaitForControlExist(waitForMilliseconds);
        }
        
        /// <summary> Waits until a specific UI control is enabled. </summary>
        /// <param name="control">Set a UI control.</param>
        /// <param name="waitForMilliseconds">Set the waiting time in milliseconds or keep default.</param>
        internal static void WaitForControlIsEnabled(UITestControl control, int waitForMilliseconds = DataHelper.DefaultWaitTime)
        {
            control.WaitForControlEnabled(waitForMilliseconds);
        }

        /// <summary> Waits until a given UI control is clickable or not clickable. </summary>
        /// <param name="control">Set a UI control to wait for.</param>
        /// <param name="waitForMilliseconds">Set a desired waiting time in milliseconds or keep default.</param>
        /// <param name="isClickable">Determine whether the expected property of the control is 'Clickable' or 'Not Clickable'.</param>
        internal static void WaitForControlIsClickable(UITestControl control, int waitForMilliseconds = DataHelper.DefaultWaitTime, bool isClickable = true)
        {
            var timer = Stopwatch.StartNew();
            for (; ; )
            {
                Playback.Wait(1000);
                if (IsClickable(control) == isClickable) break;
                if ((int) timer.ElapsedMilliseconds > waitForMilliseconds)
                    throw new Exception(String.Format("The given control '{0}' 'IsClickable' property is not '{1}' after {2} seconds!", control.FriendlyName, isClickable, waitForMilliseconds / 1000));
            }
            timer.Stop();
        }

        /// <summary>Waits for a specified row at a given results screen.</summary>
        /// <param name="rowName">Set a row name.</param>
        /// <param name="testContext"> Set current Test Context.</param>
        /// <param name="waitForMilliseconds">Set wait time in milliseconds.</param>
        private static void WaitForResultsRow(string rowName, TestContext testContext, int waitForMilliseconds)
        {
            UITestControl row = null;
            CodedUITestMain useMain = new CodedUITestMain();
            var timer = Stopwatch.StartNew();
            for ( ; ; )
            {
                Playback.Wait(1000);

                if (IsGroupedByFiles()) row = useMain.ScanResultsByFilesTable().GetRow(rowName);
                else row = useMain.ScanResultsByComputersTable().GetRow(rowName);
                
                if (row != null)
                {
                    testContext.WriteLine(String.Format("The given row '{0}' is found at the results screen.", rowName));
                    break;
                }
                if ((int) timer.ElapsedMilliseconds > waitForMilliseconds)
                    throw new UITestControlNotFoundException(String.Format("The given row '{0}' is not found at the results screen after {1} seconds!", rowName, waitForMilliseconds / 1000));
            }
            timer.Stop();
        }

        /// <summary>Waits for a specified row at a given results screen.</summary>
        /// <param name="row">Set a row.</param>
        /// <param name="testContext">Set current test context.</param>
        /// <param name="waitForMilliseconds">Set wait time in milliseconds.</param>
        internal static void WaitForResultsRow(IRow row, TestContext testContext, int waitForMilliseconds = DataHelper.DefaultWaitTime)
        {
            WaitForResultsRow(row.RowName, testContext, waitForMilliseconds);            
        }

        /// <summary>Waits for a specified row to be absent at a given results screen.</summary>
        /// <param name="rowName">Set a row name.</param>
        /// <param name="resultsScreen">Set results screen.</param>
        /// <param name="testContext"> Set current Test Context.</param>
        /// <param name="waitForMilliseconds">Set wait time in milliseconds.</param>
        internal static void WaitForAbsentResultsRow(IRow row, ResultsScreen resultsScreen, TestContext testContext, int waitForMilliseconds = DataHelper.DefaultWaitTime)
        {
            UITestControl expectedRow = null;
            CodedUITestMain useMain = new CodedUITestMain();

            var timer = Stopwatch.StartNew();
            for (; ; )
            {
                Playback.Wait(1000);
                switch (resultsScreen)
                {
                    case ResultsScreen.ScanResults:
                        {
                            expectedRow = useMain.ScanResultsTab().GetRow(row.RowName);
                            break;
                        }
                    case ResultsScreen.ValkyrieAnalysis:
                        {
                            expectedRow = useMain.ValkyrieAnalysisResultsTab().GetRow(row.RowName);
                            break;
                        }
                }
                if (expectedRow == null)
                {
                    testContext.WriteLine(String.Format("The given row '{0}' is not found at '{1}'.", row.RowName, resultsScreen));
                    break;
                }
                if ((int) timer.ElapsedMilliseconds > waitForMilliseconds)
                    throw new Exception(String.Format("The given row '{0}' is still present at '{1}' after {2} seconds!", row.RowName, resultsScreen, waitForMilliseconds / 1000));
            }
            timer.Stop();
        }

        /// <summary> Determines whether a given UI control is cliackable. </summary>
        /// <param name="control">Set a UI control.</param>
        /// <returns>True, if the UI control is clickable.</returns>
        internal static bool IsClickable(this UITestControl control)
        {
            Point point;
            return control.TryGetClickablePoint(out point);
        }

        /// <summary> Gets rows from a given tab page control. </summary>
        /// <param name="tab">Set a tab page control.</param>
        /// <param name="resultsScreen">Set a results screen type.</param>
        internal static UITestControl[] GetRows(this WpfTabPage tab)
        {
            if (tab == null) throw new Exception("The provided WPF tab is null!");

            int treeShift = 2;
            if (IsGroupedByFiles()) treeShift++;
            
            var table = tab.GetChildren()[treeShift].GetChildren()[0] as WpfTable;
            if (table == null) throw new Exception("The resulting WPF table is null!");

            var rows = table.GetRows();

            return rows;
        }

        /// <summary> Gets rows from a given table control. </summary>
        /// <param name="table">Set a table control.</param>
        internal static UITestControl[] GetRows(this WpfTable table)
        {
            if (table == null) throw new Exception("The provided WPF table is null!");

            var rowsCollection = table.GetChildren()[1].GetChildren();

            return rowsCollection.ToArray();
        }
        
        /// <summary> Gets a given row name. In accordance with Application.Models.Apt.ThreatInfoItem or Core.FileResults. </summary>
        /// <param name="row">Set a row control.</param>
        internal static string GetRowName(this UITestControl row)
        {
            if (row == null) throw new Exception("The provided row is null!");
            return row.Name;
        }

        /// <summary> Gets a given row status. </summary>
        /// <param name="row">Set a row.</param>
        internal static string GetRowStatus(this UITestControl row)
        {
            int statusShift = 1;
            if (!IsGroupedByFiles()) statusShift++;

            var cell = row.GetChildren()[1];
            var cellElement = cell.GetChildren()[0];
            var text = cellElement.GetChildren()[statusShift];

            return text.Name;
        }

        /// <summary> Gets a content of given cell in a provided row. </summary>
        /// <param name="row">Set a row.</param>
        /// <param name="cellNumber">Set a cell number.</param>
        private static string GetCellContent(this UITestControl row, int cellNumber)
        {
            var cells = row.GetChildren();
            var cell = cells.FirstOrDefault(c => c.GetChildren().Any()); // Finds the first nonempty cell in the row
            if (cell == null) throw new Exception("There is no nonempty cell in the given row!");
            cellNumber += cells.IndexOf(cell) - 1; // Corrects the given cell index in the row

            var cellElement = cells[cellNumber].GetChildren()[0];
            int iterations = 4;
            for (int i = 1; i < iterations; i++)
            {
               if (cellElement is WpfText) break;
               cellElement = cellElement.GetChildren()[0];
               if (i == iterations) throw new Exception(String.Format("No WPF text found in a given cell #{0} after {1} iterations!", cellNumber, iterations));
            }

            WpfText text = cellElement as WpfText;

            return text.DisplayText;
        }

        /// <summary> Gets a content of given cell in a provided row. </summary>
        /// <param name="resultsScreen">Set a results screen.</param>
        /// <param name="row">Set a row.</param>
        /// <param name="cellNumber">Set a cell number.</param>
        internal static string GetCellContent(ResultsScreen resultsScreen, IRow row, int cellNumber)
        {
            CodedUITestMain useMain = new CodedUITestMain();
            UITestControl rowControl;

            switch (resultsScreen)
            {
                case ResultsScreen.ScanResults:
                    rowControl = useMain.ScanResultsTab().GetRow(row.RowName);
                    break;
                case ResultsScreen.ValkyrieAnalysis:
                    rowControl = useMain.ValkyrieAnalysisResultsTab().GetRow(row.RowName);
                    break;
                default: throw new Exception(String.Format("Unexpected results screen '{0}'!", resultsScreen));
            }

            return rowControl.GetCellContent(cellNumber);
        }

        /// <summary> Waits for a given cell content in a provided row. </summary>
        /// <param name="resultsScreen">Set a results screen.</param>
        /// <param name="row">Set a row.</param>
        /// <param name="cellNumber">Set a cell number.</param>
        /// <param name="expectedContent">Set an expected content to wait for.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        /// <param name="waitTime">Set waiting time in milliseconds.</param>
        internal static void WaitForCellContent(ResultsScreen resultsScreen, IRow row, int cellNumber, string expectedContent, TestContext testContext, int waitTime)
        {
            var timer = Stopwatch.StartNew();
            string successMessage = String.Format("Expected content '{0}' appeared for row '{1}'.", expectedContent, row.RowName);
            string failureMessage = String.Format("Expected content '{0}' did not appear for row '{1}' after {2} second(s)!", expectedContent, row.RowName, waitTime / 1000);
            string actualStatus = null;

            for (;;)
            {
                Playback.Wait(2000);                
                try // The row may be in the state of refreshing and therefore may have no child controls, if a screen is automatically refreshed
                {
                    actualStatus = GetCellContent(resultsScreen, row, cellNumber);
                }
                catch (Exception) { }

                if (String.Equals(actualStatus, expectedContent, StringComparison.OrdinalIgnoreCase))
                {
                    testContext.WriteLine(successMessage);
                    break;
                }

                if ((int) timer.ElapsedMilliseconds > waitTime)
                {
                    Assert.Fail(failureMessage);
                    break;
                }
            }
            timer.Stop();
        }

        /// <summary> Gets a current scan status for a given row. </summary>
        /// <param name="row">Set a row.</param>
        private static ScanStatus GetScanStatus(this UITestControl row)
        {
            string rowStatus = row.GetRowStatus();

            foreach (var str in Settings.Results.Default.ScanStatusIndicatorRunning) if (rowStatus.Contains(str)) return ScanStatus.InProgress;
            if (rowStatus.Contains(Settings.Results.Default.ScanStatusIndicatorCompleted)) return ScanStatus.Completed;
            else if (rowStatus.Contains(Settings.Results.Default.ScanStatusIndicatorCanceled)) return ScanStatus.Canceled;
            else if (rowStatus.Contains(Settings.Results.Default.ScanStatusIndicatorFailed)) return ScanStatus.Failed;
            else if (rowStatus.Contains(Settings.Results.Default.ScanStatusIndicatorNotAccessible)) return ScanStatus.NotAccessible;
            else if (rowStatus.Contains(Settings.Results.Default.ScanStatusIndicatorConnecting)) return ScanStatus.Connecting;
            else throw new Exception(String.Format("No known scan status indicators found in the given row status '{0}'!", rowStatus));
        }

        /// <summary> Gets a current scan status at UI for a given computer. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        internal static ScanStatus GetScanStatus(this Computer endpoint)
        {
            UITestControl row;
            CodedUITestMain useMain = new CodedUITestMain();
            if (useMain.ScanResultsTab().Exists) row = useMain.ScanResultsTab().GetRow(endpoint.RowName);
            else row = useMain.ScanResultsByComputersTable().GetRow(endpoint.RowName);

            return row.GetScanStatus();
        }

        /// <summary> Gets a current full scan status message for a given computer. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        internal static string GetFullScanStatusMessage(this Computer endpoint)
        {
            UITestControl row;
            CodedUITestMain useMain = new CodedUITestMain();
            if (useMain.ScanResultsTab().Exists) row = useMain.ScanResultsTab().GetRow(endpoint.RowName);
            else row = useMain.ScanResultsByComputersTable().GetRow(endpoint.RowName);

            return row.GetRowStatus();
        }

        /// <summary> Waits for an expected scan status of an indicated computer. </summary>
        /// <param name="endpoint">Set a computer.</param>
        /// <param name="expectedScanStatus">Set an expected scan status.</param>
        /// <param name="testContext">Set a current Test Context.</param>
        /// <param name="waitTime">Set waiting time in milliseconds.</param>
        internal static void WaitForScanStatus(Computer endpoint, ScanStatus expectedScanStatus, TestContext testContext, int waitTime = DataHelper.DefaultWaitTime)
        {
            var timer = Stopwatch.StartNew();
            for (; ; )
            {
                Playback.Wait(2000);
                if (endpoint.GetScanStatus() == expectedScanStatus)
                {
                    testContext.WriteLine(String.Format("The expected status '{0}' appeared for computer '{1}'.", expectedScanStatus, endpoint.Name));
                    break;
                }
                if ((int) timer.ElapsedMilliseconds > waitTime)
                    throw new Exception(String.Format("The expected status '{0}' did not appear for computer '{1}' after {2} seconds!", expectedScanStatus, endpoint.Name, waitTime / 1000));
            }
            timer.Stop();
        }

        /// <summary> Gets a row with a given name from the indicated tab page. Returns null, if a row with the given name was not found on the tab page. </summary>
        /// <param name="tab">Set a tab page.</param>
        /// <param name="rowName">Set a unique row name.</param>
        internal static UITestControl GetRow(this WpfTabPage tab, string rowName)
        {
           var rows = tab.GetRows();            
           foreach (var row in rows) if (String.Compare(row.GetRowName(), rowName, true) == 0) return row;
           
           return null;
        }

        /// <summary> Gets a row with a given name from the indicated table. Returns null, if a row with the given name was not found in the tree. </summary>
        /// <param name="table">Set a table.</param>
        /// <param name="rowName">Set a unique row name.</param>
        internal static UITestControl GetRow(this WpfTable table, string rowName)
        {
            var rows = table.GetRows();

            foreach (var row in rows) if (String.Compare(row.GetRowName(), rowName, true) == 0) return row;

            return null;
        }

        /// <summary> Clicks a given row at a specified results screen. Starts at Results screen. </summary>
        ///<param name="screen">Set a results screen.</param>
        /// <param name="rowName">Set a unique row name.</param>
        /// <param name="click">Set a mouse click type.</param>
        private static void ClickRow(string rowName, MouseClick click)
        {
            WpfText wpfText;
            UITestControl[] rows;
            CodedUITestMain useMain = new CodedUITestMain();

            if (IsGroupedByFiles()) rows = useMain.ScanResultsByFilesTable().GetRows();
            else rows = useMain.ScanResultsByComputersTable().GetRows();

            foreach (var row in rows)
            {
                if (String.Compare(row.GetRowName(), rowName, true) == 0)
                {
                    var text = row.GetChildren()[1].GetChildren()[0].GetChildren()[0];
                    //var text = row.GetChildren().FirstOrDefault(c => c is WpfText); // First WPF text in the row
                    wpfText = text as WpfText;
                    if (wpfText == null) throw new Exception("The resulting WPF text is null!");
                    if (click == MouseClick.DoubleLeft) Mouse.DoubleClick(wpfText);
                    else if (click == MouseClick.Right) Mouse.Click(wpfText, MouseButtons.Right);
                    else Mouse.Click(wpfText);
                    return;
                }
            }
            throw new UITestControlNotFoundException(String.Format("Row '{0}' not found at results screen!", rowName));
        }

        /// <summary> Clicks a given row at a specified results screen. Starts at Results screen. </summary>
        /// <param name="row">Set a row.</param>
        /// <param name="click">Set a mouse click type.</param>
        internal static void ClickRow(IRow row, MouseClick click = MouseClick.Left)
        {
            ClickRow(row.RowName, click);
        }

        /// <summary> Looks for a given threat at a provided results screen. The indicated screen should be opened. </summary>
        /// <param name="row">Set a threat.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        internal static void FindThreat(Threat threat, TestContext testContext)
        {
            CodedUITestMain useMain = new CodedUITestMain();

            GroupByFiles();
            useMain.UIMapMain.InputTextToSearchBoxParams.UISearchWatermarkTextBEditSendKeys = threat.Name;
            useMain.UIMapMain.InputTextToSearchBox();

            try
            {
                WaitForResultsRow(threat, testContext);
                useMain.UIMapMain.ClickClearSearchTextButton();
                GroupByComputers();
            } catch(UITestControlNotFoundException)
            {
                useMain.UIMapMain.ClickClearSearchTextButton();
                GroupByComputers();
                Assert.Fail(String.Format("Row '{0}' not found at the results screen!", threat.RowName));
            }
        }

        /// <summary> Checks whether a given threat is absent at a provided results screen. The indicated screen should be opened. </summary>
        /// <param name="row">Set a threat.</param>        
        /// <param name="testContext">Set a current TestContext.</param>
        internal static void VerifyAbsentThreat(Threat threat, TestContext testContext)
        {
            CodedUITestMain useMain = new CodedUITestMain();

            GroupByFiles();
            useMain.UIMapMain.InputTextToSearchBoxParams.UISearchWatermarkTextBEditSendKeys = threat.Name;
            useMain.UIMapMain.InputTextToSearchBox();

            try
            {
                WaitForResultsRow(threat, testContext);
                useMain.UIMapMain.ClickClearSearchTextButton();
                GroupByComputers();
                Assert.Fail(String.Format("Row '{0}' found at the results screen, not expected!", threat.RowName));
            }
            catch (UITestControlNotFoundException)
            {
                useMain.UIMapMain.ClickClearSearchTextButton();
                GroupByComputers();
                testContext.WriteLine(String.Format("Row '{0}' not found at the results screen as expected.", threat.RowName));
            }
        }

        /// <summary> Determines whether there is a given threat instance at a defined results screen. </summary>
        /// <param name="threat">Set a threat.</param>
        internal static bool IsThereThreatInstance(Threat threat)
        {
            CodedUITestMain useMain = new CodedUITestMain();
            UITestControl[] rows = null;

            GroupByFiles();
            useMain.UIMapMain.InputTextToSearchBoxParams.UISearchWatermarkTextBEditSendKeys = threat.Name;
            useMain.UIMapMain.InputTextToSearchBox();

            if (IsGroupedByFiles()) rows = useMain.ScanResultsByFilesTable().GetRows();
            else rows = useMain.ScanResultsByComputersTable().GetRows();

            foreach (var row in rows)
            {
                string instanceName = row.GetRowStatus();
                if (String.Compare(instanceName, threat.Location, true) == 0)
                {
                    useMain.UIMapMain.ClickClearSearchTextButton();
                    GroupByComputers();
                    return true;
                }
            }

            useMain.UIMapMain.ClickClearSearchTextButton();
            GroupByComputers();
            return false;            
        }

        /// <summary> Switches the results screens to 'Group by Files' view. Starts at Main screen. </summary>
        internal static void GroupByFiles()
        {
            CodedUITestMain useMain = new CodedUITestMain();
            useMain.UIMapResultsWindow.ClickGroupByFilesToggleButton();
        }

        /// <summary> Switches the results screens to 'Group by Computers' view. Starts at Main screen. </summary>
        internal static void GroupByComputers()
        {
            CodedUITestMain useMain = new CodedUITestMain();
            useMain.UIMapResultsWindow.ClickGroupByComputersToggleButton();
        }

        /// <summary> Determines whether the results screen data are grouped by files. Starts at the results screen. </summary>        
        internal static bool IsGroupedByFiles()
        {
            CodedUITestMain useMain = new CodedUITestMain();
            return useMain.GroupByFilesToggleButton().Pressed;
        }

        /// <summary> Asserts whether a browser window is opened with a given URI and closes the respective window. </summary>
        /// <param name="expectedUri">Set an expected URI.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        internal static void AssertBrowserUri(string expectedUri, TestContext testContext)
        {
            BrowserWindow webPage = new BrowserWindow();
            string error = null;
            webPage.SearchProperties.Add(new PropertyExpression(BrowserWindow.PropertyNames.Name, expectedUri, PropertyExpressionOperator.Contains));
            WaitForControl(webPage);
            string actualUri = webPage.Uri.ToString();
            if (actualUri.Contains(expectedUri)) testContext.WriteLine(String.Format("Browser window opened with a correct address: '{0}'.", actualUri));
            else error = String.Format("Browser window opened with an incorrect address: '{0}'!", actualUri);                        
            webPage.Close();

            if (!String.IsNullOrEmpty(error)) Assert.Fail(error);
        }
    }
}
