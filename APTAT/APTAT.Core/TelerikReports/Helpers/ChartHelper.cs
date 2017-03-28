using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core.TelerikReports.Helpers
{
    public class ChartHelper
    {
        public static double CalculateSpacingSlotCount(int barsCount, double barWidthInPercent)
        {
            if (barsCount == 0) throw new ArgumentException(nameof(barsCount));
            if (barWidthInPercent == 0) throw new ArgumentException(nameof(barWidthInPercent));

            return 100 / (barsCount * barWidthInPercent) -1;
        }

        public double CalculateBarSize(int barsCount, double spacingSlotCount)
        {
            double bars = barsCount + barsCount * spacingSlotCount;
            if (bars == 0) return 0;
            return 100.0 / bars;
        }
    }
}
