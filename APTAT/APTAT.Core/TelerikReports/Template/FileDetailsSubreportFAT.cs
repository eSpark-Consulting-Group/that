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
    /// Summary description for FileDetailsSubreport.
    /// </summary>
    public partial class FileDetailsSubreportFAT : Telerik.Reporting.Report
    {
        public FileDetailsSubreportFAT()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        #region Expressions Helpers

        


        public static Color ColorFromFileType(BadFileType fileType)
        {
            switch (fileType)
            {
                case BadFileType.Unknown:
                    return ColorTranslator.FromHtml("#FCC700");
                case BadFileType.Infected:
                    return ColorTranslator.FromHtml("#D7474A");
                case BadFileType.NotAnalyzed:
                    return ColorTranslator.FromHtml("#c5ced5");
                case BadFileType.Pending:
                    return ColorTranslator.FromHtml("#5285C2");
                    
            }
            return Color.Black;
        }

        public static string FileTypeName(BadFileType badFileType)
        {
            switch (badFileType)
            {
                case BadFileType.Unknown:
                    return "Unknown Files";
                case BadFileType.Infected:
                    return "Malicious";
                case BadFileType.NotAnalyzed:
                    return "Not Analyzed";
                case BadFileType.Pending:
                    return "In Analysis";
                default:
                    throw new NotSupportedException(nameof(badFileType));
            }
        }

        public static int FileTypeGroupOrder(BadFileType badFileType)
        {
            switch (badFileType)
            {
                case BadFileType.Infected:
                    return 0;
                case BadFileType.Unknown:
                    return 1;
                case BadFileType.Pending:
                    return 2;
                case BadFileType.NotAnalyzed:

                    return 3;
                default:
                    throw new NotSupportedException(nameof(badFileType));

            }
        }
        #endregion
        private void FileDetailsValkyrieSubreport_ItemDataBinding(object sender, EventArgs e)
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

            Bindings.Add(new Telerik.Reporting.Binding("DataSource", "=ReportItem.Parent.DataObject." + ds));
        }
    }
}