using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace APTAT.Application
{
    public class DesignGrid : Panel
    {
        public Double CellSize { get; set; }



        public DesignGrid()
        {
            CellSize = 48;  

            Loaded += DesignGridLoaded;
        }

        private void DesignGridLoaded(Object sender, RoutedEventArgs e)
        {
            for(Int32 i = 0; i < 150; i++)
            {
                Children.Add(new Rectangle {Fill = new SolidColorBrush(Color.FromArgb(0x15, 0xFF, 0x30, 0x30))});
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count < 150)
            {
                return new Size(0, 0);
            }

            Double actualW = 12 * CellSize;
            Double actualH = 12 * CellSize;
            
            

            for (Int32 i = 0; i < 12; ++i )
            {
                for(Int32 j = 0; j < 12; ++j)
                {
                    UIElement child = Children[i * 10 + j];
                    child.Arrange(new Rect(j * 1.5 * CellSize, i * 1.5 * CellSize, CellSize, CellSize));
                }
            }

            return new Size(actualW, actualH);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Double requiredW = 0;
            Double requiredH = 0;
            
            foreach (UIElement child in Children)
            {
                child.Measure(new Size(CellSize, CellSize));

                requiredW += child.DesiredSize.Width;
                requiredH += child.DesiredSize.Height;
            }

            return new Size(requiredW, requiredH);


        }
    }
}
