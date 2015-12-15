using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using APTAT.Application.Models;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for CamCredentialsWindow.xaml
    /// </summary>
    public partial class CamCredentialsWindow : Window
    {
        private readonly ValkyrieRegistrationModel _model;
        private readonly Brush _defaultBorderBrush;
        public CamCredentialsWindow(ValkyrieRegistrationModel model)
        {
            InitializeComponent();
            _model = model;
            this.RegistrationFormLayout.DataContext = model;
            
            _defaultBorderBrush = PasswordText.BorderBrush;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public void MarkPasswordValidation(bool isValid)
        {
            if (isValid)
            {
                this.PasswordText.BorderBrush = _defaultBorderBrush;
            }
            else
            {
                this.PasswordText.BorderBrush  = new SolidColorBrush(Colors.Red);
            }
        }

        public event EventHandler OkClick;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnOkClick();
        }

        public string LicenseId
        {
            get { return this.LicenseIdText.Text.Trim(); }
        }

        public string UserName
        {
            get { return this.LoginText.Text.Trim(); }
        }

        private void GetValkyrieButton_OnClick(object sender, RoutedEventArgs e)
        {
            var resource = TryFindResource("GETValkyrieLink");
            Process.Start(new ProcessStartInfo(resource.ToString()));
            e.Handled = true;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            _model.Password = this.PasswordText.Password.Trim();
//            PasswordText.InvalidateVisual();
        }

        protected virtual void OnOkClick()
        {
            var handler = OkClick;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
