using System.Collections.Generic;
using APTAT.Core.Reports;

namespace APTAT.Core.TelerikReports.Template
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;

    /// <summary>
    /// Summary description for Devices.
    /// </summary>
    public partial class DeviceDetailsSubReport : Report
    {
        private  List<Telerik.Reporting.Panel> panels;
        public DeviceDetailsSubReport()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            InitPanelsCollection();

        }

        private void InitPanelsCollection()
        {
            panels = new List<Telerik.Reporting.Panel>
            {
                panel01IpAddress,
                panel02OperationSystem,
                panel03OrganizationUnit,
                panel04FilesScanned,
                panel05TotalFilesFound,
                panel06,
                panel07,
                panel08,
                panel09,
                panel010,
                panel011,
                panel012
            };
        }

        private void Devices_ItemDataBinding(object sender, EventArgs e)
        {
            var ds = ReportParameters["DataSourceBinding"].Value.ToString();
            if (Bindings.Count > 0)
            {
                var bind = Bindings.FirstOrDefault(b => b.Path == "DataSource");
                if (bind != null)
                {
                    Bindings.Remove(bind);
                }
            }

            Bindings.Add(new Telerik.Reporting.Binding("DataSource", "=ReportItem.Parent.DataObject." + ds));



            var obj = (Telerik.Reporting.Processing.Report)sender;


            var dataSource = (Reports.AptReportData)obj.Parent.DataObject.RawData; 

            
            RowsColorAlterating(dataSource);
        }

        private void RowsColorAlterating(AptReportData dataSource)
        {
            var bgColor = Color.FromArgb(251, 251, 251);
            var isActiveDirectory = dataSource.DevicesSourceDescription.Type ==
                                    DeploymentCore.Directory.ETargetsDiscovererType.ActiveDirectory;
            List<Telerik.Reporting.Panel> panelToColor;
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
    }
}