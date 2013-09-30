using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace APTAT.Application
{
    public class SelectorButton : Button
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header",
            typeof(Object),
            typeof(SelectorButton),
            new PropertyMetadata(null));

        public Object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public SelectorButton()
        {
            DefaultStyleKey = typeof(SelectorButton);
        }

    }
}
