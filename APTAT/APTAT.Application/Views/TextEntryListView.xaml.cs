using System;

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;


using System.Collections.Specialized;

namespace APTAT.Application.Views
{
    public class CancelItemArgs
    {
        public object Item { get; set; }

        public bool Cancel { get; set; }
    }

    public partial class TextEntryListView : UserControl
    {
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate",
            typeof(DataTemplate),
            typeof(TextEntryListView),
            new PropertyMetadata(null, ItemTemplatePropertyChangedCallback));

        private static void ItemTemplatePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (TextEntryListView)d;
            view.ItemsList.ItemTemplate = (DataTemplate)e.NewValue;
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(object),
            typeof(TextEntryListView),
            new PropertyMetadata(null, ItemsSourcePropertyChangedCallback));

        private static void ItemsSourcePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (TextEntryListView)d;
            var collectionSource = (CollectionViewSource)view.LayoutRoot.Resources["CollectionViewSource"];
            collectionSource.Source = e.NewValue;

            var oldCollection = e.OldValue as INotifyCollectionChanged;
            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= view.SourceCollectionChanged;
            }

            var newCollection = e.NewValue as INotifyCollectionChanged;
            if (newCollection != null)
            {
                newCollection.CollectionChanged += view.SourceCollectionChanged;
            }
        }

        public static readonly DependencyProperty AddCommandProperty = DependencyProperty.Register(
            "AddCommand",
            typeof(ICommand),
            typeof(TextEntryListView),
            null);

        public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(
            "RemoveCommand",
            typeof(ICommand),
            typeof(TextEntryListView),
            null);

        public static readonly DependencyProperty RemoveAllCommandProperty = DependencyProperty.Register(
            "RemoveAllCommand",
            typeof(ICommand),
            typeof(TextEntryListView),
            null);



        private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ItemsList.SelectedItem = e.NewItems[0];
                RemoveAllButton.IsEnabled = true;
            }
            else if (!ItemsList.HasItems)
                RemoveAllButton.IsEnabled = false;
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public object ItemsSource
        {
            get { return GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public ICommand AddCommand
        {
            get { return (ICommand)GetValue(AddCommandProperty); }
            set { SetValue(AddCommandProperty, value); }
        }

        public ICommand RemoveCommand
        {
            get { return (ICommand)GetValue(RemoveCommandProperty); }
            set { SetValue(RemoveCommandProperty, value); }
        }

        public ICommand RemoveAllCommand
        {
            get { return (ICommand)GetValue(RemoveAllCommandProperty); }
            set { SetValue(RemoveAllCommandProperty, value); }
        }

        public TextEntryListView()
        {
            InitializeComponent();

            AddButton.Click += AddButton_Click;
            RemoveButton.Click += RemoveButton_Click;
            RemoveAllButton.Click += RemoveAllButton_Click;
            InputTextBox.KeyDown += InputTextBox_KeyDown;
            InputTextBox.MouseEnter += InputTextBox_MouseEnter;
            InputTextBox.MouseLeave += InputTextBox_MouseLeave;
            ItemsList.KeyDown += ItemsList_KeyDown;
        }

        private void InputTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (tooltip != null && tooltip.IsOpen)
                tooltip.IsOpen = false;
        }

        private ToolTip tooltip;

        private void InputTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            string toolTipText = @"Note: The following templates of IP addresses or hostnames are allowed (for example).
IP             : 10.0.0.1
IP Range  : 10.0.0.1-10.0.0.5
IP Subnet : 10.0.0.0/24 -or- 10.0.0.0/255.255.255.0
Computer Name: HomeComputer";

            if (tooltip == null)
                tooltip = new ToolTip { Content = toolTipText };
            tooltip.VerticalOffset = 2;
            tooltip.PlacementTarget = InputTextBox;
            tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            if (!tooltip.IsOpen)
                tooltip.IsOpen = true;

        }



        private void ListLoaded(object sender, RoutedEventArgs e)
        {
            Action focus = () =>
            {
                Keyboard.Focus(InputTextBox);
                InputTextBox.Focus();

            };

            Dispatcher.BeginInvoke(focus);
        }




        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (tooltip != null && tooltip.IsOpen)
                tooltip.IsOpen = false;

            var bExp = InputTextBox.GetBindingExpression(TextBox.TextProperty);
            if (bExp != null)
                bExp.UpdateSource();

            var args = new CancelItemArgs
            {
                Item = InputTextBox.Text,
                Cancel = false,
            };

            if (AddCommand != null && AddCommand.CanExecute(args))
                AddCommand.Execute(args);

            if (!args.Cancel)
            {
                InputTextBox.Text = String.Empty;
                ItemsList.ScrollIntoView(args.Item);
            }

            InputTextBox.Focus();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Object> items = from Object selectedItem in ItemsList.SelectedItems select selectedItem;
            var args = new CancelItemArgs
            {

                Item = items.ToList(),
                Cancel = false,
            };

            if (RemoveCommand != null && RemoveCommand.CanExecute(args))
                RemoveCommand.Execute(args);
        }

        private void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Object> items = from Object selectedItem in ItemsList.Items select selectedItem;
            var args = new CancelItemArgs
            {

                Item = items.ToList(),
                Cancel = false,
            };

            if (RemoveAllCommand != null && RemoveAllCommand.CanExecute(args))
                RemoveAllCommand.Execute(args);
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {


            if (e.Key == Key.Enter)
            {
                AddButton_Click(this, new RoutedEventArgs());
            }
        }

        private void ItemsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                RemoveButton_Click(this, new RoutedEventArgs());
            }
        }
    }
}
