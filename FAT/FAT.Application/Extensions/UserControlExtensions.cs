using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Telerik.Windows.Controls;

namespace FAT.Application.Extensions
{
    public static class UserControlExtensions
    {
        public static void CreateDetailsHeader(this RadMenuItem menuItem)
        {
            CreateHeaderDataTemplate(menuItem, "Details", "M11,14 L17,14 C17.553,14 18,14.447 18,15 18,15.553 17.553,16 17,16 L11,16 C10.447,16 10,15.553 10,15 10,14.447 10.447,14 11,14 z M7.0000001,14 L7.9999999,14 C8.5530014,14 9,14.447006 9,15 9,15.552994 8.5530014,16 7.9999999,16 L7.0000001,16 C6.4469986,16 6,15.552994 6,15 6,14.447006 6.4469986,14 7.0000001,14 z M11,11 L17,11 C17.553,11 18,11.448 18,12 18,12.552 17.553,13 17,13 L11,13 C10.447,13 10,12.552 10,12 10,11.448 10.447,11 11,11 z M7.0000001,11 L7.9999999,11 C8.553,11 9,11.448 9,12 9,12.552 8.553,13 7.9999999,13 L7.0000001,13 C6.447,13 6,12.552 6,12 6,11.448 6.447,11 7.0000001,11 z M11,8 L17,8 C17.553,8 18,8.448 18,9 18,9.552 17.553,10 17,10 L11,10 C10.447,10 10,9.552 10,9 10,8.448 10.447,8 11,8 z M7.0000001,8 L7.9999999,8 C8.553,8 9,8.448 9,9 9,9.552 8.553,10 7.9999999,10 L7.0000001,10 C6.447,10 6,9.552 6,9 6,8.448 6.447,8 7.0000001,8 z M12,2 C6.478,2 2,6.4780002 2,12 2,17.522 6.478,22 12,22 17.522,22 22,17.522 22,12 22,6.4780002 17.522,2 12,2 z M12,0 C18.627,0 24,5.3730001 24,12 24,18.627 18.627,24 12,24 5.373,24 0,18.627 0,12 0,5.3730001 5.373,0 12,0 z");
        }

        public static void CreateRestartScanHeader(this RadMenuItem menuItem)
        {
            CreateHeaderDataTemplate(menuItem, "Restart Scan", "M10,7.0000001 L16,12 10,17 z M12,0 C18.627,0 24,5.373 24,12 24,18.627 18.627,24 12,24 L12,22 C17.522,22 22,17.522 22,12 22,6.478 17.522,2 12,2 6.4779999,2 2,6.478 2,12 2,14.251 2.7550001,16.319 4.0110002,17.989 L7,15 7,22 0,22 2.5939999,19.406 C0.97999954,17.366 0,14.804 0,12 0,5.373 5.3730001,0 12,0 z");
        }

        public static void CreateRestartScanWithCredentialsHeader(this RadMenuItem menuItem)
        {
            CreateHeaderDataTemplate(menuItem, "Restart Scan With Credentials", "M12,9 C11.447006,9 11,9.447998 11,10 L11,12 13,12 13,10 C13,9.447998 12.552994,9 12,9 z M12,7.0000001 C13.656998,7 15,8.3430023 15,10 L15,12 16,12 16,17 7.9999999,17 7.9999999,12 9,12 9,10 C9,8.3430023 10.343002,7 12,7.0000001 z M12,0 C18.627,0 24,5.373 24,12 24,18.627 18.627,24 12,24 L12,22 C17.522,22 22,17.522 22,12 22,6.478 17.522,2 12,2 6.478,2 2,6.478 2,12 2,14.251 2.7550001,16.318 4.0110002,17.989 L7.0000001,15 7.0000001,22 0,22 2.5939999,19.406 C0.98000002,17.365 0,14.804 0,12 0,5.373 5.3729999,0 12,0 z");
        }

        public static void CreateStopScanHeader(this RadMenuItem menuItem)
        {
            CreateHeaderDataTemplate(menuItem, "Stop Scan", "M7.9999999,7.9999999 L16,7.9999999 16,16 7.9999999,16 z M12,2 C6.478,2 2,6.478 2,12 2,17.522 6.478,22 12,22 17.522,22 22,17.522 22,12 22,6.478 17.522,2 12,2 z M12,0 C18.627,0 24,5.3729999 24,12 24,18.627 18.627,24 12,24 5.3729999,24 0,18.627 0,12 0,5.3729999 5.3729999,0 12,0 z");
        }

        private static void CreateHeaderDataTemplate(RadMenuItem menuItem, string header, string icon)
        {
            var template = new DataTemplate();
            var stack = new FrameworkElementFactory(typeof(StackPanel));
            stack.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var path = new FrameworkElementFactory(typeof(Path));

            path.SetValue(Path.DataProperty, Geometry.Parse(icon));
            path.SetValue(Path.FillProperty, new SolidColorBrush(Color.FromArgb(255, 42, 128, 185)));
            path.SetValue(Path.HeightProperty, 24d);
            path.SetValue(Path.WidthProperty, 24d);
            path.SetValue(Path.MarginProperty, new Thickness(0, 5, 5, 5));
            path.SetValue(Path.SnapsToDevicePixelsProperty, true);

            var text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetValue(TextBlock.MarginProperty, new Thickness(5, 0, 5, 0));
            text.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            text.SetBinding(TextBlock.TextProperty, new Binding());
            text.SetValue(TextBlock.FontSizeProperty, 15d);
            text.SetValue(TextBlock.FontFamilyProperty, new FontFamily("myriadpro"));

            stack.AppendChild(path);
            stack.AppendChild(text);

            template.VisualTree = stack;

            menuItem.Margin = new Thickness(-1);
            menuItem.HeaderTemplate = template;
            menuItem.Header = header;
        }

        public static void SetVisibilityBinding(this RadMenuItem menuItem)
        {
            var binding = new Binding("IsEnabled");
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.Self);
            binding.Converter = new Telerik.Windows.Controls.BooleanToVisibilityConverter();
            BindingOperations.SetBinding(menuItem, UIElement.VisibilityProperty, binding);
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
