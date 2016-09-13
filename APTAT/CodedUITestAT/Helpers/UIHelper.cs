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

namespace CodedUITestAT
{
    /// <summary> Contains general methods for work with UI. </summary>
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

        /// <summary> Waits until a given UI control is clickable. </summary>
        /// <param name="control">Set a UI control to wait for.</param>
        /// <param name="waitForMilliseconds">Set a desired waiting time in milliseconds or keep default.</param>
        internal static void WaitForControlIsClickable(UITestControl control, int waitForMilliseconds = DataHelper.DefaultWaitTime)
        {
            var timer = Stopwatch.StartNew();
            for (; ; )
            {
                Playback.Wait(1000);
                if (IsClickable(control)) break;
                if ((int) timer.ElapsedMilliseconds > waitForMilliseconds)
                    throw new Exception(String.Format("The given control '{0}' is not clickable after {1} seconds!", control.FriendlyName, waitForMilliseconds / 1000));
            }
            timer.Stop();
        }

        /// <summary>Waits for a specified row at a given results screen.</summary>
        /// <param name="rowName">Set a row name.</param>
        /// <param name="resultsScreen">Set results screen.</param>
        /// <param name="testContext"> Set current Test Context.</param>
        /// <param name="waitForMilliseconds">Set wait time in milliseconds.</param>
        private static void WaitForResultsRow(string rowName, ResultsScreen resultsScreen, TestContext testContext, int waitForMilliseconds)
        {
            UITestControl row = null;
            CodedUITestMain useMain = new CodedUITestMain();
            var timer = Stopwatch.StartNew();
            for ( ; ; )
            {
                Playback.Wait(1000);                
                switch (resultsScreen)
                {
                    case ResultsScreen.ScanningResults:
                        {
                            row = useMain.ScanningResultsTab().GetRow(rowName);
                            break;
                        }
                    case ResultsScreen.ValkyrieAnalysis:
                        {
                            row = useMain.ValkyrieAnalysisResultsTab().GetRow(rowName);
                            break;
                        }
                }
                if (row != null)
                {
                    testContext.WriteLine(String.Format("The given row '{0}' is found at '{1}'.", rowName, resultsScreen));
                    break;
                }
                if ((int) timer.ElapsedMilliseconds > waitForMilliseconds)
                    throw new UITestControlNotFoundException(String.Format("The given row '{0}' is not found at '{1}' after {2} seconds!", rowName, resultsScreen, waitForMilliseconds / 1000));
            }
            timer.Stop();
        }

        /// <summary>Waits for a specified row at a given results screen.</summary>
        /// <param name="row">Set a row.</param>
        /// <param name="resultsScreen">Set results screen.</param>
        /// <param name="testContext">Set current test context.</param>
        /// <param name="waitForMilliseconds">Set wait time in milliseconds.</param>
        internal static void WaitForResultsRow(IRow row, ResultsScreen resultsScreen, TestContext testContext, int waitForMilliseconds = DataHelper.DefaultWaitTime)
        {
            WaitForResultsRow(row.RowName, resultsScreen, testContext, waitForMilliseconds);            
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
                    case ResultsScreen.ScanningResults:
                        {
                            expectedRow = useMain.ScanningResultsTab().GetRow(row.RowName);
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
        internal static bool IsClickable(UITestControl control)
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
            if (DataHelper.IsGroupedByFiles) treeShift++;
            
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
        
        /// <summary> Gets a given row name. In accordance with Application.Models.ThreatInfoItem or Core.FileResults. </summary>
        /// <param name="row">Set a row control.</param>
        internal static string GetRowName(this UITestControl row)
        {
            return row.GetChildren()[1].Name.ToLower();
        }

        /// <summary> Gets a given row status. </summary>
        /// <param name="row">Set a row.</param>
        internal static string GetRowStatus(this UITestControl row)
        {
            var cell = row.GetChildren()[1];
            var cellElement = cell.GetChildren()[0];
            var text = cellElement.GetChildren()[2];

            return text.Name;
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
            if (useMain.ScanningResultsTab().Exists) row = useMain.ScanningResultsTab().GetRow(endpoint.RowName);
            else row = useMain.ScanningResultsTable().GetRow(endpoint.RowName);

            return row.GetScanStatus();
        }

        /// <summary> Gets a current full scan status message for a given computer. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        internal static string GetFullScanStatusMessage(this Computer endpoint)
        {
            UITestControl row;
            CodedUITestMain useMain = new CodedUITestMain();
            if (useMain.ScanningResultsTab().Exists) row = useMain.ScanningResultsTab().GetRow(endpoint.RowName);
            else row = useMain.ScanningResultsTable().GetRow(endpoint.RowName);

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
                    testContext.WriteLine(String.Format("The expected status '{0}' appeared for computer '{1}'.\n", expectedScanStatus, endpoint.Name));
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

           foreach (var row in rows) if (row.GetRowName().Contains(rowName)) return row;
           
           return null;
        }

        /// <summary> Gets a row with a given name from the indicated table. Returns null, if a row with the given name was not found in the tree. </summary>
        /// <param name="table">Set a table.</param>
        /// <param name="rowName">Set a unique row name.</param>
        internal static UITestControl GetRow(this WpfTable table, string rowName)
        {
            var rows = table.GetRows();

            foreach (var row in rows) if (row.GetRowName().Contains(rowName)) return row;

            return null;
        }

        /// <summary> Clicks a given row at a specified results screen. Starts at Results screen. </summary>
        ///<param name="screen">Set a results screen.</param>
        /// <param name="rowName">Set a unique row name.</param>
        /// <param name="click">Set a mouse click type.</param>
        private static void ClickRow(ResultsScreen screen, string rowName, MouseClick click)
        {
            WpfButton expandRowButton;
            UITestControl[] rows;
            CodedUITestMain useMain = new CodedUITestMain();

            switch (screen)
            {
                case ResultsScreen.ScanningResults:
                    if (useMain.ScanningResultsTab().Exists)
                    {
                        useMain.UIMapResultsWindow.GoToScanningResultsTab();
                        rows = useMain.ScanningResultsTab().GetRows();
                    }
                    else rows = useMain.ScanningResultsTable().GetRows();                    
                    break;
                case ResultsScreen.ValkyrieAnalysis:
                    useMain.UIMapResultsWindow.GoToValkyrieAnalysisResultsTab();
                    rows = useMain.ValkyrieAnalysisResultsTab().GetRows();
                    rowName = Path.GetFileName(rowName);
                    break;
                default: throw new Exception(String.Format("Unexpected results screen '{0}'!", screen));
            }

            foreach (var row in rows)
            {
                if (row.GetRowName().Contains(rowName))
                {
                    expandRowButton = row.GetChildren()[0] as WpfButton;
                    if (click == MouseClick.DoubleLeft) Mouse.DoubleClick(expandRowButton);
                    else if (click == MouseClick.Right) Mouse.Click(expandRowButton, MouseButtons.Right);
                    else Mouse.Click(expandRowButton);
                    return;
                }
            }
            throw new UITestControlNotFoundException(String.Format("Row '{0}' not found at screen '{1}'!", rowName, screen));
        }

        /// <summary> Clicks a given row at a specified results screen. Starts at Results screen. </summary>
        ///<param name="screen">Set a results screen.</param>
        /// <param name="row">Set a row.</param>
        /// <param name="click">Set a mouse click type.</param>
        internal static void ClickRow(ResultsScreen screen, IRow row, MouseClick click = MouseClick.Left)
        {
            ClickRow(screen, row.RowName, click);
        }

        /// <summary> Looks for a given row at a provided results screen. The indicated screen should be opened. </summary>
        /// <param name="row">Set a row.</param>
        /// <param name="resultsScreen">Set a results screen.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        internal static void FindRow(IRow row, ResultsScreen resultsScreen, TestContext testContext)
        {
            CodedUITestMain useMain = new CodedUITestMain();

            GroupByFiles();
            useMain.UIMapMain.InputTextToSearchBoxParams.UISearchTextBoxEditText = Path.GetFileName(row.RowName);
            useMain.UIMapMain.InputTextToSearchBox();

            try
            {
                WaitForResultsRow(row, resultsScreen, testContext);
                useMain.UIMapMain.ClickClearSearchTextButton();
                GroupByComputers();
            }
            catch(UITestControlNotFoundException)
            {
                useMain.UIMapMain.ClickClearSearchTextButton();
                GroupByComputers();
                Assert.Fail(String.Format("Row '{0}' not found at screen '{1}'!", row.RowName, resultsScreen));
            }
        }

        /// <summary> Switches the results screens to 'Group by Files' view. Starts at Main screen. </summary>
        internal static void GroupByFiles()
        {
            CodedUITestMain useMain = new CodedUITestMain();
            useMain.UIMapResultsWindow.ClickGroupByFilesToggleButton();
            DataHelper.IsGroupedByFiles = true;
        }

        /// <summary> Switches the results screens to 'Group by Computers' view. Starts at Main screen. </summary>
        internal static void GroupByComputers()
        {
            CodedUITestMain useMain = new CodedUITestMain();
            useMain.UIMapResultsWindow.ClickGroupByComputersToggleButton();
            DataHelper.IsGroupedByFiles = false;
        }
    }
}
