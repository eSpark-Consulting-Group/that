using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Telerik.Windows.Controls;

namespace APTAT.Application.Extensions
{
    public static class UserControlExtensions
    {
        public static void CreateHeaderDataTemplate(this RadMenuItem menuItem, string header)
        {
            var template = new DataTemplate();
            var text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetBinding(TextBlock.TextProperty, new Binding());
            text.SetValue(TextBlock.FontSizeProperty, 16d);
            text.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Segoe UI Light"));
            template.VisualTree = text;
            menuItem.HeaderTemplate = template;
            menuItem.Header = header;
        }

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
    }
}
