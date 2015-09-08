using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorLogWindow: Window
    {
        public ErrorLogWindow(IEnumerable<string> errors)
        {
            InitializeComponent();
        
            SourceInitialized += (x, y) =>
            {
                this.HideMinimizeAndMaximizeButtons();
            };

            var errorMessages = errors as string[] ?? errors.ToArray();
            ErrorHeader.Text = errorMessages.Length > 1 ? "Errors" : "Error";
            foreach (var errorMessage in errorMessages)
            {
                var itm = new ListBoxItem {Content = errorMessage};
                ErrorList.Items.Add(itm);
            }
            
        }

       
        private void OnCloseClick(Object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void YesClick(Object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void NoClick(Object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
