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
    public partial class Devices : Telerik.Reporting.Report
    {
        public Devices()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        private void Devices_ItemDataBinding(object sender, EventArgs e)
        {
            var ds = this.ReportParameters["DataSourceBinding"].Value.ToString();
            if (Bindings.Count > 0)
            {
                var bind = Bindings.FirstOrDefault(b => b.Path == "DataSource");
                if (bind != null)
                {
                    Bindings.Remove(bind);
                }
            }

            this.Bindings.Add(new Telerik.Reporting.Binding("DataSource", "=ReportItem.Parent.DataObject." + ds));


        }
    }
}