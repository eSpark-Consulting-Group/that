using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow 
    {
        public ReportWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            SourceInitialized += (x, y) =>
            {
                this.HideMinimizeAndMaximizeButtons();
            };
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OnUnloaded(Object sender, RoutedEventArgs e)
        {
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(TitleProperty, typeof (Window));
            if (dpd != null)
            {
                dpd.RemoveValueChanged(this, Changed);
            }
        }

        private void OnLoaded(Object sender, RoutedEventArgs e)
        {
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(TitleProperty, typeof(Window));
            if (dpd != null)
            {
                dpd.AddValueChanged(this, Changed);
            }
        }

        private void Changed(Object sender, EventArgs e)
        {
            if (txtTitle != null)
            {
                txtTitle.Text = Title;
            }
        }
    }
}
