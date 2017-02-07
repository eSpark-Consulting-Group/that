
using FAT.Application.Extensions;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;

namespace FAT.Application.Views
{
    public partial class SearchUserControl
    {
        public SearchUserControl()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                BindingOperations.SetBinding(this, PredicateProperty, new Binding("FilterPredicate"));
                BindingOperations.SetBinding(this, FilterOrdersProperty, new Binding("FilterOrders"));
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

            if (!string.IsNullOrEmpty(value))
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
            }

            if (string.IsNullOrEmpty(value) && control.FilterOrders.Length == 0)
            {
                control.Target?.FilterDescriptors.Clear();
            }
        }

        #endregion

        #region FilterOrdersProperty (DependencyProperty)
        public int[] FilterOrders
        {
            get { return (int[])GetValue(FilterOrdersProperty); }
            set { SetValue(FilterOrdersProperty, value); }
        }

        public static readonly DependencyProperty FilterOrdersProperty =
            DependencyProperty.Register("FilterOrders", typeof(int[]), typeof(SearchUserControl),
            new PropertyMetadata(null, OnFilterOrdersChanged));

        private static void OnFilterOrdersChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as SearchUserControl;
            var value = e.NewValue as int[];

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

            if (value.Length == 0 && string.IsNullOrEmpty(control.Keyword))
                control.Target.FilterDescriptors.Clear();
        }

        #endregion

        #region WatermarkProperty (DependencyProperty)
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string), typeof(SearchUserControl),
            new PropertyMetadata(null, OnWatermarkChanged));

        private static void OnWatermarkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as SearchUserControl;
            var value = e.NewValue as string;
            (control.search.WatermarkContent as TextBlock).Text = value;
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            search.Text = null;
            search.DelayedFocus();
        }

    }
}