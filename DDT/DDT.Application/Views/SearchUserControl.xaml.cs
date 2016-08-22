
using System.Windows;
using DDT.Application.Extensions;
using Telerik.Windows.Controls;
using System;
using System.ComponentModel;
using System.Windows.Data;
using Telerik.Windows.Data;
using System.Windows.Input;

namespace DDT.Application.Views
{
    public partial class SearchUserControl
    {
        public SearchUserControl()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                BindingOperations.SetBinding(this, PredicateProperty, new Binding("SearchPredicate"));
            }
        }

        #region TargetProperty (DependencyProperty)

        public RadTreeListView Target
        {
            get { return (RadTreeListView)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(RadTreeListView), typeof(SearchUserControl),
            new PropertyMetadata(null));

        #endregion

        #region PdfTargetProperty (DependencyProperty)

        public RadPdfViewer PdfTarget
        {
            get { return (RadPdfViewer)GetValue(PdfTargetProperty); }
            set { SetValue(PdfTargetProperty, value); }
        }

        public static readonly DependencyProperty PdfTargetProperty =
            DependencyProperty.Register("PdfTarget", typeof(RadPdfViewer), typeof(SearchUserControl),
            new PropertyMetadata(null));

        #endregion

        #region PredicateProperty (DependencyProperty)

        public Predicate<object> Predicate
        {
            get { return (Predicate<object>)GetValue(PredicateProperty); }
            set { SetValue(PredicateProperty, value); }
        }

        public static readonly DependencyProperty PredicateProperty =
            DependencyProperty.Register("Predicate", typeof(Predicate<object>), typeof(SearchUserControl),
            new PropertyMetadata(null));

        #endregion

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

            if (!string.IsNullOrEmpty(value) && value.Length > 2)
            {
                if (control.Target != null)
                {
                    if (control.Target.FilterDescriptors.Count == 0)
                        control.Target.FilterDescriptors.Add(new FilterDescriptor<object>
                        {
                            FilteringExpression = _ => control.Predicate(_)
                        });
                    else
                        control.Target.FilterDescriptors.Reset();
                }
                else if (control.PdfTarget != null)
                {
                    control.PdfTarget.CommandDescriptors.FindCommandDescriptor.Command.Execute(value);
                }
            }
            else if (control.Target != null)
                control.Target.FilterDescriptors.Clear();
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            search.Text = null;
            search.DelayedFocus();
        }

        private void search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(Keyword) && Keyword.Length > 2 && PdfTarget != null)
                PdfTarget.CommandDescriptors.FindCommandDescriptor.Command.Execute(Keyword);
        }
    }
}