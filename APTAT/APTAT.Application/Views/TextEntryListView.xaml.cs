using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;


using System.Collections.Specialized;

namespace APTAT.Application
{
    public class CancelItemArgs
    {
        public object Item { get; set; }

        public bool Cancel { get; set; }
    }

	public partial class TextEntryListView: UserControl
	{
		public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
			"ItemTemplate",
			typeof(DataTemplate),
			typeof(TextEntryListView),
			new PropertyMetadata(null, ItemTemplatePropertyChangedCallback));

		private static void ItemTemplatePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var view = (TextEntryListView) d;
            view.ItemsList.ItemTemplate = (DataTemplate)e.NewValue;
		}

		public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
			"ItemsSource",
			typeof(object),
			typeof(TextEntryListView),
			new PropertyMetadata(null, ItemsSourcePropertyChangedCallback));

		private static void ItemsSourcePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var view = (TextEntryListView) d;
			var collectionSource = (CollectionViewSource) view.LayoutRoot.Resources["CollectionViewSource"];
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

		public static readonly DependencyProperty TipProperty = DependencyProperty.Register(
			"Tip",
			typeof(string),
			typeof(TextEntryListView),
			new PropertyMetadata(null, TipPropertyChangedCallback));

		private static void TipPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var view = (TextEntryListView) d;

            ToolTipService.SetToolTip(view.ToolTipIcon, e.NewValue);
			//view.TipText.Content = e.NewValue;
		}

        private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ItemsList.SelectedItem = e.NewItems[0];
            }
        }

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate) GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		public object ItemsSource
		{
			get { return GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public ICommand AddCommand
		{
			get { return (ICommand) GetValue(AddCommandProperty); }
			set { SetValue(AddCommandProperty, value); }
		}

		public ICommand RemoveCommand
		{
			get { return (ICommand) GetValue(RemoveCommandProperty); }
			set { SetValue(RemoveCommandProperty, value); }
		}

		public string Tip
		{
			get { return (string) GetValue(TipProperty); }
			set { SetValue(TipProperty, value); }
		}

		public TextEntryListView()
		{
			InitializeComponent();

			AddButton.Click += AddButton_Click;
			RemoveButton.Click += RemoveButton_Click;
			InputTextBox.KeyDown += InputTextBox_KeyDown;
			ItemsList.KeyDown += ItemsList_KeyDown;
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
