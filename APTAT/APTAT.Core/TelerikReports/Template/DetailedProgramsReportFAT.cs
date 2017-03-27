namespace APTAT.Core.TelerikReports
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;

    /// <summary>
    /// Summary description for ExecutiveReportt.
    /// </summary>
    public partial class DetailedProgramsReportFAT : Telerik.Reporting.Report
    {
        public DetailedProgramsReportFAT()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        private void graph1_ItemDataBinding(object sender, EventArgs e)
        {
            var graph = (Telerik.Reporting.Processing.DataItem)sender;
            var obj =(Reports.AptReportData)graph.DataObject.RawData;
            var len = obj.Top10PopularBadFiles.Length;
            var spaceSlotCount = (11 - len) * 0.2;

            if (len == 1) spaceSlotCount = 10;
            if (len == 2) spaceSlotCount = 4;
            if (len == 3) spaceSlotCount = 2;
            ((CategoryScale)graphAxis2.Scale).SpacingSlotCount = spaceSlotCount;
        }
    }
}