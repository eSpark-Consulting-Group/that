using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Windows;

using System.Windows.Input;


namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow 
    {
        public ReportWindow()
        {
            InitializeComponent();



        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }




    }
}
