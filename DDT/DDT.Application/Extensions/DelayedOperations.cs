using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace DDT.Application.Extensions
{
    public static class DelayedOperations
    {
        public static void DelayedFocus(this UIElement uiElement)
        { 
            uiElement.Dispatcher.BeginInvoke(
            new Action(delegate
            {
                if (uiElement.Focusable)
                {
                    uiElement.Focus();
                    Keyboard.Focus(uiElement);
                    if (uiElement is TextBox)
                        ((TextBox)uiElement).CaretIndex = 
                        ((TextBox)uiElement).Text.Length;
                }
            }),
            DispatcherPriority.Render);
        }

        public static void DelayedToggle(this ToggleButton uiElement, bool? value)
        {
            uiElement.Dispatcher.BeginInvoke(
            new Action(delegate
            {
                uiElement.IsChecked = value;
            }),
            DispatcherPriority.Render);
        }
    }
}
