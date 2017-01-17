using System.Reflection;
using System.Windows;
using FAT.Application.Extensions;
using System.Windows.Input;
using System;

namespace FAT.Application.Views
{
    public partial class LoginPasswordUserControl
    {
        public LoginPasswordUserControl()
        {
            InitializeComponent();
        
        }

        #region LoginProperty (DependencyProperty)

        public string Login
        {
            get { return (string)GetValue(LoginProperty); }
            set { SetValue(LoginProperty, value); }
        }

        public static readonly DependencyProperty LoginProperty =
            DependencyProperty.Register("Login", typeof(string), typeof(LoginPasswordUserControl),
            new PropertyMetadata(string.Empty));

        #endregion

        #region PasswordProperty (DependencyProperty)

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(LoginPasswordUserControl),
            new PropertyMetadata(string.Empty, OnPasswordChanged));

        private static void OnPasswordChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as LoginPasswordUserControl;
            var value = e.NewValue as string;

            if (control.OpenedEye.Visibility == Visibility.Visible)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (control.PasswordMasked.Visibility == Visibility.Visible)
                        return;
                    control.PasswordMasked.Visibility = Visibility.Visible;
                    control.PasswordMasked.IsTabStop = true;
                    control.PasswordMasked.Password = value;
                    control.SetPasswordBoxSelection(value.Length);
                }
                else
                {
                    if (control.PasswordMasked.Visibility == Visibility.Collapsed)
                        return;
                    control.PasswordMasked.Visibility = Visibility.Collapsed;
                    control.PasswordMasked.IsTabStop = false;
                    control.PasswordUnmasked.DelayedFocus();
                }
            }
        }

        #endregion

        #region UseCurrentDomain (DependencyProperty)

        public bool UseCurrentDomain
        {
            get { return (bool)GetValue(UseCurrentDomainProperty); }
            set { SetValue(UseCurrentDomainProperty, value); }
        }

        public static readonly DependencyProperty UseCurrentDomainProperty =
            DependencyProperty.Register("UseCurrentDomain", typeof(bool), typeof(LoginPasswordUserControl),
            new PropertyMetadata(false));

        #endregion

        #region ShowCurrentDomain (DependencyProperty)

        public bool ShowCurrentDomain
        {
            get { return (bool)GetValue(ShowCurrentDomainProperty); }
            set { SetValue(ShowCurrentDomainProperty, value); }
        }

        public static readonly DependencyProperty ShowCurrentDomainProperty =
            DependencyProperty.Register("ShowCurrentDomain", typeof(bool), typeof(LoginPasswordUserControl),
            new PropertyMetadata(false));

        #endregion

        #region ShowDomainController (DependencyProperty)

        public bool ShowDomainController
        {
            get { return (bool)GetValue(ShowDomainControllerProperty); }
            set { SetValue(ShowDomainControllerProperty, value); }
        }

        public static readonly DependencyProperty ShowDomainControllerProperty =
            DependencyProperty.Register("ShowDomainController", typeof(bool), typeof(LoginPasswordUserControl),
            new PropertyMetadata(false));

        #endregion



        private void SwitchMask(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordUnmasked.Text))
                return;

            if (OpenedEye.Visibility == Visibility.Visible)
            {
                ClosedEye.Visibility = Visibility.Visible;
                PasswordMasked.Visibility = OpenedEye.Visibility = Visibility.Collapsed;
                PasswordMasked.IsTabStop = false;

                PasswordUnmasked.DelayedFocus();
            }
            else
            {
                ClosedEye.Visibility = Visibility.Collapsed;
                PasswordMasked.Visibility = OpenedEye.Visibility = Visibility.Visible;
                PasswordMasked.IsTabStop = true;

                if (string.IsNullOrEmpty(PasswordUnmasked.Text))
                {
                    PasswordMasked.Password = string.Empty;
                    PasswordMasked.DelayedFocus();
                }
                else
                {
                    PasswordMasked.Password = PasswordUnmasked.Text;
                    SetPasswordBoxSelection(PasswordUnmasked.Text.Length);
                }
            }
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordUnmasked.Text = PasswordMasked.Password;
        }

        private void SetPasswordBoxSelection(int start)
        {
            PasswordMasked.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic).
                Invoke(PasswordMasked, new object[] { start, 0 });
            PasswordMasked.DelayedFocus();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (ShowDomainController)
                    text.DelayedFocus();
                else
                    text2.DelayedFocus();
            });
        }
    }
}