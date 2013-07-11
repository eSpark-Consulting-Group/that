using System.ComponentModel;
using System.Runtime.InteropServices;
using APTAT.Application.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace APTAT.Application
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
      

        public MainWindow()
        {
            InitializeComponent();

            DispatcherHelper.Current = Dispatcher;

            MouseLeftButtonDown += MainWindow_MouseDown;

            SourceInitialized += (x, y) =>
            {
                this.HideMinimizeAndMaximizeButtons();
            };
            

            Loaded += MainWindow_Loaded;
            
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(TitleProperty, typeof(Window));
            if (dpd != null)
            {
                dpd.AddValueChanged(this, delegate
                {
                    if (txtTitle != null)
                    {
                        txtTitle.Text = Title;
                    }
                });
            }
        }

        

        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
           
                WindowState = WindowState.Normal;
                DragMove();
               
           
        }

   

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnMinimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
