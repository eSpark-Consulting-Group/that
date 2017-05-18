namespace APTAT.Core.TelerikReports
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;
    using APTAT.Core.TelerikReports.Helpers;

    /// <summary>
    /// Summary description for ExecutiveReportt.
    /// </summary>
    public partial class DetailedDeviceReportFAT : Telerik.Reporting.Report
    {
        public DetailedDeviceReportFAT()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
  

        }

        private void DetailedDevicesReportFAT_ItemDataBound(object sender, EventArgs e)
        {
          
            
        }

        private void DetailedDeviceReportFAT_ItemDataBinding(object sender, EventArgs e)
        {
            var obj = (Telerik.Reporting.Processing.Report)sender;
            var dataSource = (Reports.AptReportData)obj.DataSource;       

            var hasBadFiles = dataSource.AllBadFilesSorted.Length > 0;
            panelSpaceBeforeDetailsPerProgram.Visible = hasBadFiles;
        }

        private void graph1_ItemDataBinding(object sender, EventArgs e)
        {
            var graph = (Telerik.Reporting.Processing.DataItem)sender;
            var obj = (Reports.AptReportData)graph.DataObject.RawData;

            if (obj.Top10VulnerableDevices.Max(t => t.BadFilesCount) < 3)
            {
                ((NumericalScale)graphAxis1.Scale).LabelStep = 1;
                ((NumericalScale)graphAxis1.Scale).DataPointTicks = true;
                ((NumericalScale)graphAxis1.Scale).Minimum = 0.1d;
            }

            var len = obj.Top10VulnerableDevices.Select(t=>t.Uid).Distinct().Count();            
            ((CategoryScale)graphAxis2.Scale).SpacingSlotCount = ChartHelper.CalculateSpacingSlotCount(len, 8);
        }
    }
}