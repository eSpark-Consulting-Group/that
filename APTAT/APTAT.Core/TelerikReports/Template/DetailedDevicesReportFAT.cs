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
            var dataSource = (Reports.AptReportDataFAT)obj.DataSource;
            var bgColor = Color.FromArgb(251, 251, 251);

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
            var len = obj.Top10VulnerableDevicesValkyrie.Select(t=>t.Uid).Distinct().Count();
            var spaceSlotCount = (11 - len) * 0.2;

            if (len == 1) spaceSlotCount = 10;
            if (len == 2) spaceSlotCount = 4;
            if (len == 3) spaceSlotCount = 2;
            ((CategoryScale)graphAxis2.Scale).SpacingSlotCount = spaceSlotCount;
        }
    }
}