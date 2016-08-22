using System;
using System.Windows;

namespace DDT.Application.Helpers
{
    public class ThemeHelper : DependencyObject
    {
        public static readonly DependencyProperty ThemeProperty =
            DependencyProperty.RegisterAttached("Theme", typeof(Uri),
            typeof(ThemeHelper),
            new UIPropertyMetadata(null, OnThemeChanged));

        public static Uri GetTheme(DependencyObject obj)
        {
            return (Uri)obj.GetValue(ThemeProperty);
        }

        public static void SetTheme(DependencyObject obj, Uri value)
        {
            obj.SetValue(ThemeProperty, value);
        }

        private static void OnThemeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is FrameworkElement)
                ApplyTheme(obj as FrameworkElement, GetTheme(obj));
        }

        private static void ApplyTheme(FrameworkElement targetElement, Uri dictionaryUri)
        {
            if (targetElement == null) return;

            ResourceDictionary themeDictionary = null;
            if (dictionaryUri != null)
            {
                themeDictionary = new ResourceDictionary();
                themeDictionary.Source = dictionaryUri;

                targetElement.Resources.MergedDictionaries.Insert(0, themeDictionary);
            }

            foreach (var dict in targetElement.Resources.MergedDictionaries)
            {
                if (themeDictionary == dict) continue;
                targetElement.Resources.MergedDictionaries.Remove(dict);
                break;
            }
        }
    }
}
