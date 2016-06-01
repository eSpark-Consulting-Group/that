using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting;
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
            var tree = tab.GetChildren()[2].GetChildren()[0] as WpfTree;
            var rows = tree.GetRows();

            return rows;
        }

        /// <summary> Gets rows from a given tree control. </summary>
        /// <param name="tab">Set a tree control.</param>
        internal static UITestControl[] GetRows(this WpfTree tree)
        {            
            var rowsCollection = tree.GetChildren()[2].GetChildren();

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

            foreach (var str in Settings.Results.Default.ScanStatusIndicatorRunning) if (rowStatus.Contains(str)) return ScanStatus.Running;
            if (rowStatus.Contains(Settings.Results.Default.ScanStatusIndicatorCompleted)) return ScanStatus.Completed;
            else if (rowStatus.Contains(Settings.Results.Default.ScanStatusIndicatorStopped)) return ScanStatus.Stopped;
            else if (rowStatus.Contains(Settings.Results.Default.ScanStatusIndicatorFailed)) return ScanStatus.Failed;
            else if (rowStatus.Contains(Settings.Results.Default.ScanStatusIndicatorNotAccessible)) return ScanStatus.NotAccessible;
            else throw new Exception(String.Format("No known scan status indicators found in the given row status '{0}'!", rowStatus));
        }

        /// <summary> Gets a current scan status at UI for a given computer. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        internal static ScanStatus GetScanStatus(this Computer endpoint)
        {
            UITestControl row;
            CodedUITestMain useMain = new CodedUITestMain();
            if (useMain.ScanningResultsTab().Exists) row = useMain.ScanningResultsTab().GetRow(endpoint.RowName);
            else row = useMain.ScanningResultsTree().GetRow(endpoint.RowName);

            return row.GetScanStatus();
        }

        /// <summary> Waits for an expected scan status of an indicated computer. </summary>
        /// <param name="endpoint">Set a computer.</param>
        /// <param name="expectedScanStatus">Set an expected scan status.</param>
        /// <param name="waitTime">Set waiting time in milliseconds.</param>
        internal static void WaitForScanStatus(Computer endpoint, ScanStatus expectedScanStatus, int waitTime = DataHelper.DefaultWaitTime)
        {
            var timer = Stopwatch.StartNew();
            for (; ; )
            {
                Playback.Wait(2000);
                if (endpoint.GetScanStatus() == expectedScanStatus) break;
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

        /// <summary> Gets a row with a given name from the indicated tree. Returns null, if a row with the given name was not found in the tree. </summary>
        /// <param name="tab">Set a tree.</param>
        /// <param name="rowName">Set a unique row name.</param>
        internal static UITestControl GetRow(this WpfTree tree, string rowName)
        {
            var rows = tree.GetRows();

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
            WpfTabPage tab;
            UITestControl[] rows;
            CodedUITestMain useMain = new CodedUITestMain();

            switch (screen)
            {
                case ResultsScreen.ScanningResults:
                    if (useMain.ScanningResultsTab().Exists)
                    {
                        useMain.UIMapResultsWindow.GoToScanningResultsTab();
                        tab = useMain.ScanningResultsTab();
                        rows = tab.GetRows();
                    }
                    else rows = useMain.ScanningResultsTree().GetRows();                    
                    break;
                case ResultsScreen.ValkyrieAnalysis:
                    useMain.UIMapResultsWindow.GoToValkyrieAnalysisResultsTab();
                    tab = useMain.ValkyrieAnalysisResultsTab();
                    rows = tab.GetRows();
                    break;
                default: throw new Exception("Unexpected Results screen!");
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
            throw new Exception(String.Format("Row '{0}' not found at screen '{1}'!", rowName, screen));
        }

        /// <summary> Clicks a given computer at a specified results screen. Starts at Results screen. </summary>
        ///<param name="screen">Set a results screen.</param>
        /// <param name="computer">Set a computer.</param>
        /// <param name="click">Set a mouse click type.</param>
        internal static void ClickRow(ResultsScreen screen, Computer computer, MouseClick click = MouseClick.Left)
        {
            ClickRow(screen, computer.RowName, click);
        }

        /// <summary> Clicks a given threat at a specified results screen. Starts at Results screen. </summary>
        ///<param name="screen">Set a results screen.</param>
        /// <param name="threat">Set a threat.</param>
        /// <param name="click">Set a mouse click type.</param>
        internal static void ClickRow(ResultsScreen screen, Threat threat, MouseClick click = MouseClick.Left)
        {
            ClickRow(screen, threat.RowName, click);
        }
    }
}
