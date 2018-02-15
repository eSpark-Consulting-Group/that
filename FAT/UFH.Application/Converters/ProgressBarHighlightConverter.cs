using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace UFH.Application.Converters
{
    public class ProgressBarHighlightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Type doubleType = typeof(double);
            if (values == null ||
                (values.Length != 3) ||
                (values[0] == null) ||
                (values[1] == null) ||
                (values[2] == null) ||
                !typeof(Brush).IsAssignableFrom(values[0].GetType()) ||
                !doubleType.IsAssignableFrom(values[1].GetType()) ||
                !doubleType.IsAssignableFrom(values[2].GetType()))
                return null;

            Brush brush = (Brush)values[0];
            double width = (double)values[1];
            double height = (double)values[2];

            if (width <= 0.0 || Double.IsInfinity(width) || Double.IsNaN(width) ||
                height <= 0.0 || Double.IsInfinity(height) || Double.IsNaN(height))
                return null;

            DrawingBrush newBrush = new DrawingBrush();
            double twiceWidth = width * 2.0;
            newBrush.Viewport = newBrush.Viewbox = new Rect(-width, 0, twiceWidth, height);
            newBrush.ViewportUnits = newBrush.ViewboxUnits = BrushMappingMode.Absolute;
            newBrush.TileMode = TileMode.None;
            newBrush.Stretch = Stretch.None;

            DrawingGroup myDrawing = new DrawingGroup();
            DrawingContext myDrawingContext = myDrawing.Open();
            myDrawingContext.DrawRectangle(brush, null, new Rect(-width, 0, width, height));

            TimeSpan translateTime = TimeSpan.FromSeconds(twiceWidth / 200.0);
            TimeSpan pauseTime = TimeSpan.FromSeconds(1.0); 

            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            animation.BeginTime = TimeSpan.Zero;
            animation.Duration = new Duration(translateTime + pauseTime);
            animation.RepeatBehavior = RepeatBehavior.Forever;
            animation.KeyFrames.Add(new LinearDoubleKeyFrame(twiceWidth, translateTime));

            TranslateTransform translation = new TranslateTransform();
            translation.BeginAnimation(TranslateTransform.XProperty, animation);

            newBrush.Transform = translation;
            myDrawingContext.Close();
            newBrush.Drawing = myDrawing;

            return newBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}