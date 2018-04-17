using System.Linq;

namespace APTAT.Core.TelerikReports
{
    using APTAT.Core.TelerikReports.Helpers;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;

    /// <summary>
    /// Summary description for ExecutiveReportt.
    /// </summary>
    public partial class DetailedProgramsReportUFH : Telerik.Reporting.Report
    {
        public DetailedProgramsReportUFH()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        internal void ChangeScaleForOne()
        {
            ((NumericalScale)graphAxis1.Scale).LabelStep = 1;
            ((NumericalScale)graphAxis1.Scale).MajorStep = 1;
        }

        private void graph1_ItemDataBinding(object sender, EventArgs e)
        {
            var graph = (Telerik.Reporting.Processing.DataItem)sender;
            var obj =(Reports.AptReportData)graph.DataObject.RawData;

            var hasBadFiles = obj.AllBadFilesSorted.Length > 0;
            panelSpaceBeforeDetailsPerProgram.Visible = hasBadFiles;
            var len = obj.Top10PopularBadFiles.Length;
            if (len > 0)
            {
                ((CategoryScale)graphAxis2.Scale).SpacingSlotCount = ChartHelper.CalculateSpacingSlotCount(len, 8);
            }

            var max = obj.Top10PopularBadFiles.Max(v=>v.Count);
            if (max < 5) ChangeScaleForOne();
            else
            {
                var step = max / 6;
                if (max % 6 > 0)
                    step++;
                ((NumericalScale)graphAxis1.Scale).LabelStep = step;
                ((NumericalScale)graphAxis1.Scale).MajorStep = step;
            }
            
        }
    }
}