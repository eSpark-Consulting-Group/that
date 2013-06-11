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

            MouseDown += MainWindow_MouseDown;

            //WindowResizer wr = new WindowResizer(this);

            //xxxSizeGrip, where xxx corresponds to the equivilent border position
            //wr.addResizerRight(rightSizeGrip);
            //wr.addResizerLeft(leftSizeGrip);
            //wr.addResizerUp(TopResizer);
           // wr.addResizerDown(BottomResizer);
            //wr.addResizerLeftUp(topLeftSizeGrip);
            //wr.addResizerRightUp(topRightSizeGrip);
            //wr.addResizerLeftDown(bottomLeftSizeGrip);
            //wr.addResizerRightDown(bottomRightSizeGrip);

            
        }

        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //DragMove();
        }
    }
}
