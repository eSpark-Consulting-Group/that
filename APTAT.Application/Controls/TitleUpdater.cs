using System;
using System.Windows;
using System.Windows.Controls;

namespace APTAT.Application
{
    public class TitleUpdater : Control
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header",
            typeof (String),
            typeof (TitleUpdater),
            new PropertyMetadata(String.Empty, OnHeaderPropertyChanged));

        public String Header
        {
            get { return (String) GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public TitleUpdater()
        {
            Loaded += TitleUpdater_Loaded;
        }

        private void TitleUpdater_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.Title = Header;
            } 
        }

        public static void OnHeaderPropertyChanged(Object d, DependencyPropertyChangedEventArgs e)
        {
            TitleUpdater updater = (TitleUpdater) d;
            Window window = Window.GetWindow(updater);
            if(window != null)
            {
                window.Title = updater.Header;    
            }
        }
    }
}
