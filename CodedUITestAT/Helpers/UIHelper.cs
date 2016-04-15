using System;
using System.Drawing;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting;

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
    }
}
