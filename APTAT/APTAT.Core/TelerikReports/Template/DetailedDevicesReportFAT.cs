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
    public partial class DetailedDevicesReportFAT : Telerik.Reporting.Report
    {
        public DetailedDevicesReportFAT()
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
            ((NumericalScale) graphAxis1.Scale).LabelStep = 1;
        }
    }
}