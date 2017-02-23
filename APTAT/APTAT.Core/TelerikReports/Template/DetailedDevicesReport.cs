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
    public partial class DetailedDevicesReport : Telerik.Reporting.Report
    {
        public DetailedDevicesReport()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        public void RotateAxisXLable()
        {
            cartesianCoordinateSystem1.XAxis.LabelAngle = 90;
        }

        public void ChangeScaleForOne()
        {
           ((NumericalScale) graphAxisX.Scale ).LabelStep= 1;
        }
       
    }
}