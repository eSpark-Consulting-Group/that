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
             panels = new List<Telerik.Reporting.Panel>();
            panels.Add(this.panel01IpAddress);
            panels.Add(this.panel02OperationSystem);
            panels.Add(this.panel03OrganizationUnit);
            panels.Add(this.panel04FilesScanned);
            panels.Add(this.panel05TotalFilesFound);
            panels.Add(this.panel06);
            panels.Add(this.panel07);
            panels.Add(this.panel08);
            panels.Add(this.panel09);
            panels.Add(this.panel010);
            panels.Add(this.panel011);
            panels.Add(this.panel012);

        }

        private readonly List<Telerik.Reporting.Panel> panels;
        internal void ChangeScaleForOne()
        {
            ((NumericalScale) graphAxis1.Scale).LabelStep = 1;
            ((NumericalScale)graphAxis1.Scale).MajorStep = 1;
        }

        private void DetailedDevicesReportFAT_ItemDataBound(object sender, EventArgs e)
        {
          
            
        }

        private void DetailedDeviceReportFAT_ItemDataBinding(object sender, EventArgs e)
        {
            var obj = (Telerik.Reporting.Processing.Report)sender;
            var dataSource = (Reports.AptReportData)obj.DataSource;
            AlteringColors(dataSource);
            var topVulnerableDevicesValk = dataSource.Top10VulnerableDevices.Select(d => d.BadFilesCount).DefaultIfEmpty().Max();
            if (topVulnerableDevicesValk <= 5)
            {
                ChangeScaleForOne();
            }

        }

        private void AlteringColors(Reports.AptReportData dataSource)
        {
            var bgColor = Color.FromArgb(251, 251, 251);

            var hasBadFiles = dataSource.AllBadFilesSorted.Length > 0;
            panelSpaceBeforeDetailsPerProgram.Visible = hasBadFiles;

            var isActiveDirectory = dataSource.DevicesSourceDescription.Type == DeploymentCore.Directory.ETargetsDiscovererType.ActiveDirectory;
            List<Telerik.Reporting.Panel> panelToColor = new List<Telerik.Reporting.Panel>();
            if (!isActiveDirectory)
            {
                panelToColor = panels.Where(p => p.Name != "panel03OrganizationUnit").ToList();
                shape5.Height = new Unit(7.5, UnitType.Cm);
            }
            else
            {
                panelToColor = panels;
                shape5.Height = new Unit(8, UnitType.Cm);
            }

            for (int i = 0; i < panelToColor.Count; i++)
            {
                var colorIndex = i % 2;
                if (colorIndex == 0)
                {
                    panelToColor[i].Style.BackgroundColor = bgColor;
                }
                else
                {
                    panelToColor[i].Style.BackgroundColor = Color.White;
                }
            }
        }

        private void graph1_ItemDataBinding(object sender, EventArgs e)
        {
            var graph = (Telerik.Reporting.Processing.DataItem)sender;
            var obj = (Reports.AptReportData)graph.DataObject.RawData;
            var len = obj.Top10VulnerableDevices.Select(t=>t.Uid).Distinct().Count();            
            ((CategoryScale)graphAxis2.Scale).SpacingSlotCount = ChartHelper.CalculateSpacingSlotCount(len, 8);
        }
    }
}