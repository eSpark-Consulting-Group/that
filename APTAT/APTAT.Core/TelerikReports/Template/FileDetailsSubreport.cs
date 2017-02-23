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
    /// Summary description for FileDetailsSubreport.
    /// </summary>
    public partial class FileDetailsSubreport : Telerik.Reporting.Report
    {
        public FileDetailsSubreport()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        private void FileDetailsSubreport_ItemDataBinding(object sender, EventArgs e)
        {
            var ds =  this.ReportParameters["DataSourceBinding"].Value.ToString();
            if (Bindings.Count > 0)
            {
                var bind = Bindings.Where(b => b.Path == "DataSource").FirstOrDefault();
                if (bind != null)
                {
                    Bindings.Remove(bind);
                }
            }          
            
            this.Bindings.Add(new Telerik.Reporting.Binding("DataSource", "=ReportItem.Parent.DataObject."+ds));
            
        }
    }
}