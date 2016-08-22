using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Threading;
using Telerik.Windows;
using Telerik.Windows.Controls;

namespace APTAT.Application.Controls
{
    public class RadMaskedTextBoxUpdateSourceBehavior : Behavior<RadMaskedTextBoxEx>
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof (String),
            typeof (RadMaskedTextBoxUpdateSourceBehavior),
            null);

        public String Value
        {
            get { return (String) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private Boolean _delay;
        private DispatcherTimer _timer;

        public Boolean UpdateOnEnter
        {
            get;
            set;
        }

        public Boolean Delay
        {
            get { return _delay; }
            set
            {
                _delay = value;
                if (_delay)
                {
                    if (_timer != null)
                    {
                        _timer.Stop();
                        _timer.Tick -= OnTick;
                    }
                    _timer = new DispatcherTimer();
                    _timer.Interval = TimeSpan.FromSeconds(1);
                    _timer.Tick += OnTick;
                }
            }
        }

        private void OnTick(Object sender, EventArgs e)
        {
            UpdateSourceInternal();
            _timer.Stop();
        }

        protected override void OnAttached()
        {
            AssociatedObject.ValueChanged += OnValueChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(Object sender, RadRoutedEventArgs e)
        {
            UpdateSource();
        }

        protected virtual void UpdateSource()
        {
            if (Delay)
            {
                _timer.Stop();
                _timer.Start();
            }
            else
            {
                UpdateSourceInternal();
            }
        }

        private void UpdateSourceInternal()
        {
            if (AssociatedObject.Value == null)
            {
                Value = null;
            }
            else
            {
                Value = AssociatedObject.Value.ToString();
            }
        }
	}
}
