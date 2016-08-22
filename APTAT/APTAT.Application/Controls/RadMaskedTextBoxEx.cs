using System;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace APTAT.Application.Controls
{
    public class RadMaskedTextBoxEx : RadMaskedTextBox
    {
        private const String ClearTextPart = "ClearTextButton";

        private Button _clearTextButton;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _clearTextButton = (Button)GetTemplateChild(ClearTextPart);
            if (_clearTextButton != null)
            {
                _clearTextButton.Click += OnClick;
            }

        }
 
        private void OnClick(Object sender, System.Windows.RoutedEventArgs e)
        {
            Value = null;
            Focus();
        }
    }
}
