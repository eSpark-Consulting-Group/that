
using System;
using System.Windows;

namespace APTAT.Application.Views
{
    public partial class SearchUserControl
    { 
        public SearchUserControl()
        {
            InitializeComponent();
        }

        #region KeywordProperty (DependencyProperty)
        public string Keyword
        {
            get { return (string)GetValue(KeywordProperty); }
            set { SetValue(KeywordProperty, value); }
        }

        public static readonly DependencyProperty KeywordProperty =
            DependencyProperty.Register("Keyword", typeof(string), typeof(SearchUserControl),
            new PropertyMetadata(null, OnKeywordChanged));

        private static void OnKeywordChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as SearchUserControl;
            var value = e.NewValue as string;
            if (string.IsNullOrEmpty(value))
                control.clear.Visibility = Visibility.Collapsed;
            else
                control.clear.Visibility = Visibility.Visible;
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            search.Text = null;
            search.Dispatcher.BeginInvoke((Action)delegate { search.Focus(); });
        }

    }
}