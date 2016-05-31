using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace APTAT.Application.Extensions
{
    public static class PortValidationExtensions
    {
        public static string ValidateTCP(this string value, out bool hasError)
        {            
            hasError = false;
            if (value.Length > 5)
            {
                hasError = true;
                return "TCP port should not be more than 65535";
            }

            Int32 i = 0;
            Int32.TryParse(value, out i);

            if (String.IsNullOrWhiteSpace(value))
            {
                hasError = true;
                return "Please Specify TCP port";
            }

            if (i > 65535)
            {
                hasError = true;
                return "TCP port should not be more than 65535";
            }

            return string.Empty;
        }

       
        public static string ValidateUDP(this string value, out bool hasError)
        {
            
            hasError = false;
            if (value.Length > 5)
            {
                hasError = true;
                return "UDP port should not be more than 65535";
            }
            Int32 i = 0;
            Int32.TryParse(value, out i);

            if (String.IsNullOrWhiteSpace(value))
            {
                hasError = true;
                return "Please Specify UDP port";
            }

            if (i > 65535)
            {
                hasError = true;
                return "UDP port should not be more than 65535";
            }

            return string.Empty;
        }

        public static void ValidateInput(object sender, TextCompositionEventArgs e)
        {            
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
            if (e.Handled) return;

            try
            {
                int i = Int32.Parse(((TextBox)sender).Text + e.Text);
                e.Handled = (i > 65535 || i < 1);                
            }
            catch (Exception ex)
            {
                e.Handled = true;                
            }           
            
        }
    }
}
