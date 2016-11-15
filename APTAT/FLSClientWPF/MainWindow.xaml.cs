using System;
using System.Windows;

namespace FLSClientWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly ViewModel _viewModel = new ViewModel();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = _viewModel;
        }


        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            _viewModel.Cancel();
            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Cancel();
        }

        private void button_Add(object sender, RoutedEventArgs e)
        {
            _viewModel.Add();
        }
    }
}
