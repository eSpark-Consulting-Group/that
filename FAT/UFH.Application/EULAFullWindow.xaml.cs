
using System;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Documents.Fixed;

namespace UFH.Application
{
    public partial class EULAFullWindow
    {
        public EULAFullWindow()
        {
            InitializeComponent();

            DataContext = App.Current.MainWindow.DataContext;
        }

        private void pdf_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            pdf.ScaleFactor = 1.19d;
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
            App.Current.Shutdown();
        }
    }
}