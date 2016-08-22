using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ValkyrieHashAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _model;
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load data by setting the CollectionViewSource.Source property:

            // Load data by setting the CollectionViewSource.Source property:
            // valkyrieAnalysisResultViewSource.Source = [generic data source]
            var ts = TaskScheduler.FromCurrentSynchronizationContext();
            _model = new MainViewModel(new ValkyrieClient(ts));
            this.DataContext = _model;

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _model.Analyze(dlg.FileName);
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message);
                    throw;
                }
                
            
            }
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
                if (dlg.ShowDialog() == true)
            {
                var fileName = dlg.FileName;
                _model.SaveToFile(fileName);
            }

        }
    }
}
