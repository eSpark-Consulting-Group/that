namespace APTAT.Core.TelerikReports.Template
{
    partial class Devices
    {
        #region Component Designer generated code
        /// <summary>
        /// Required method for telerik Reporting designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Telerik.Reporting.Drawing.FormattingRule formattingRule1 = new Telerik.Reporting.Drawing.FormattingRule();
            Telerik.Reporting.Drawing.FormattingRule formattingRule2 = new Telerik.Reporting.Drawing.FormattingRule();
            Telerik.Reporting.Drawing.FormattingRule formattingRule3 = new Telerik.Reporting.Drawing.FormattingRule();
            Telerik.Reporting.Drawing.FormattingRule formattingRule4 = new Telerik.Reporting.Drawing.FormattingRule();
            Telerik.Reporting.Drawing.FormattingRule formattingRule5 = new Telerik.Reporting.Drawing.FormattingRule();
            Telerik.Reporting.Group group1 = new Telerik.Reporting.Group();
            Telerik.Reporting.ReportParameter reportParameter1 = new Telerik.Reporting.ReportParameter();
            Telerik.Reporting.ReportParameter reportParameter2 = new Telerik.Reporting.ReportParameter();
            Telerik.Reporting.ReportParameter reportParameter3 = new Telerik.Reporting.ReportParameter();
            Telerik.Reporting.Drawing.StyleRule styleRule1 = new Telerik.Reporting.Drawing.StyleRule();
            this.groupFooterSection = new Telerik.Reporting.GroupFooterSection();
            this.groupHeaderSection = new Telerik.Reporting.GroupHeaderSection();
            this.textBox44 = new Telerik.Reporting.TextBox();
            this.pageHeaderSection1 = new Telerik.Reporting.PageHeaderSection();
            this.detail = new Telerik.Reporting.DetailSection();
            this.shape1 = new Telerik.Reporting.Shape();
            this.textBoxPath = new Telerik.Reporting.TextBox();
            this.textBox2 = new Telerik.Reporting.TextBox();
            this.textBox3 = new Telerik.Reporting.TextBox();
            this.textBox5 = new Telerik.Reporting.TextBox();
            this.textBox11 = new Telerik.Reporting.TextBox();
            this.textBox10 = new Telerik.Reporting.TextBox();
            this.textBox9 = new Telerik.Reporting.TextBox();
            this.textBox16 = new Telerik.Reporting.TextBox();
            this.textBox15 = new Telerik.Reporting.TextBox();
            this.textBox14 = new Telerik.Reporting.TextBox();
            this.textBox21 = new Telerik.Reporting.TextBox();
            this.textBox20 = new Telerik.Reporting.TextBox();
            this.textBox19 = new Telerik.Reporting.TextBox();
            this.htmlTextBox1 = new Telerik.Reporting.HtmlTextBox();
            this.textBox6 = new Telerik.Reporting.TextBox();
            this.pageFooterSection1 = new Telerik.Reporting.PageFooterSection();
            this.dsModule = new Telerik.Reporting.ObjectDataSource();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // groupFooterSection
            // 
            this.groupFooterSection.Height = Telerik.Reporting.Drawing.Unit.Inch(0.0520833320915699D);
            this.groupFooterSection.Name = "groupFooterSection";
            // 
            // groupHeaderSection
            // 
            this.groupHeaderSection.Height = Telerik.Reporting.Drawing.Unit.Inch(0.30224737524986267D);
            this.groupHeaderSection.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.textBox44});
            this.groupHeaderSection.Name = "groupHeaderSection";
            // 
            // textBox44
            // 
            this.textBox44.CanShrink = true;
            this.textBox44.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0D), Telerik.Reporting.Drawing.Unit.Cm(0D));
            this.textBox44.Name = "textBox44";
            this.textBox44.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(15.100000381469727D), Telerik.Reporting.Drawing.Unit.Cm(0.699999988079071D));
            this.textBox44.Style.Color = System.Drawing.Color.DimGray;
            this.textBox44.Style.Font.Name = "Segoe UI Semibold";
            this.textBox44.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(13.5D);
            this.textBox44.Value = "=\"Devices (\" +  Parameters.TargetsCount.Value + \"):\"";
            // 
            // pageHeaderSection1
            // 
            this.pageHeaderSection1.Height = Telerik.Reporting.Drawing.Unit.Cm(0.13229171931743622D);
            this.pageHeaderSection1.Name = "pageHeaderSection1";
            // 
            // detail
            // 
            this.detail.Height = Telerik.Reporting.Drawing.Unit.Inch(1.7449179887771606D);
            this.detail.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.shape1,
            this.textBoxPath,
            this.textBox2,
            this.textBox3,
            this.textBox5,
            this.textBox11,
            this.textBox10,
            this.textBox9,
            this.textBox16,
            this.textBox15,
            this.textBox14,
            this.textBox21,
            this.textBox20,
            this.textBox19,
            this.htmlTextBox1,
            this.textBox6});
            this.detail.Name = "detail";
            // 
            // shape1
            // 
            formattingRule1.Filters.Add(new Telerik.Reporting.Filter("= CInt(Fields.TargetVerdict)", Telerik.Reporting.FilterOperator.Equal, "0"));
            formattingRule1.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(206)))), ((int)(((byte)(213)))));
            formattingRule1.Style.Color = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(206)))), ((int)(((byte)(213)))));
            formattingRule2.Filters.Add(new Telerik.Reporting.Filter("= CInt(Fields.TargetVerdict)", Telerik.Reporting.FilterOperator.Equal, "1"));
            formattingRule2.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(199)))), ((int)(((byte)(54)))));
            formattingRule2.Style.Color = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(199)))), ((int)(((byte)(54)))));
            formattingRule3.Filters.Add(new Telerik.Reporting.Filter("= CInt(Fields.TargetVerdict)", Telerik.Reporting.FilterOperator.Equal, "2"));
            formattingRule3.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(71)))), ((int)(((byte)(74)))));
            formattingRule3.Style.Color = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(71)))), ((int)(((byte)(74)))));
            formattingRule4.Filters.Add(new Telerik.Reporting.Filter("= CInt(Fields.TargetVerdict)", Telerik.Reporting.FilterOperator.Equal, "3"));
            formattingRule4.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(199)))), ((int)(((byte)(0)))));
            formattingRule4.Style.Color = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(199)))), ((int)(((byte)(0)))));
            formattingRule5.Filters.Add(new Telerik.Reporting.Filter("=Fields.TargetVerdict", Telerik.Reporting.FilterOperator.Equal, "4"));
            formattingRule5.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(82)))), ((int)(((byte)(133)))), ((int)(((byte)(194)))));
            formattingRule5.Style.Color = System.Drawing.Color.FromArgb(((int)(((byte)(82)))), ((int)(((byte)(133)))), ((int)(((byte)(194)))));
            this.shape1.ConditionalFormatting.AddRange(new Telerik.Reporting.Drawing.FormattingRule[] {
            formattingRule1,
            formattingRule2,
            formattingRule3,
            formattingRule4,
            formattingRule5});
            this.shape1.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.026458332315087318D), Telerik.Reporting.Drawing.Unit.Cm(0.16770848631858826D));
            this.shape1.Name = "shape1";
            this.shape1.ShapeType = new Telerik.Reporting.Drawing.Shapes.EllipseShape(0D);
            this.shape1.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(0.20000000298023224D), Telerik.Reporting.Drawing.Unit.Cm(0.20000000298023224D));
            this.shape1.Style.BackgroundColor = System.Drawing.Color.CornflowerBlue;
            this.shape1.Style.BorderColor.Default = System.Drawing.Color.Transparent;
            this.shape1.Style.BorderWidth.Default = Telerik.Reporting.Drawing.Unit.Point(0D);
            this.shape1.Style.Color = System.Drawing.Color.Transparent;
            // 
            // textBoxPath
            // 
            this.textBoxPath.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.29999992251396179D), Telerik.Reporting.Drawing.Unit.Cm(0.017708463594317436D));
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(15.108960151672363D), Telerik.Reporting.Drawing.Unit.Cm(0.49999994039535522D));
            this.textBoxPath.Style.Font.Name = "Segoe UI Light";
            this.textBoxPath.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(11D);
            this.textBoxPath.Value = "= Fields.HostName";
            // 
            // textBox2
            // 
            this.textBox2.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.026458332315087318D), Telerik.Reporting.Drawing.Unit.Cm(0.7585417628288269D));
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.3735418319702148D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox2.Style.Color = System.Drawing.Color.DimGray;
            this.textBox2.Style.Font.Name = "Segoe UI Light";
            this.textBox2.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(10D);
            this.textBox2.Value = "IP Address:\t";
            // 
            // textBox3
            // 
            this.textBox3.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.026458332315087318D), Telerik.Reporting.Drawing.Unit.Cm(1.2965278625488281D));
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.9733412265777588D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox3.Style.Color = System.Drawing.Color.DimGray;
            this.textBox3.Style.Font.Name = "Segoe UI Light";
            this.textBox3.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(10D);
            this.textBox3.Value = "Operating System:";
            // 
            // textBox5
            // 
            this.textBox5.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.026458332315087318D), Telerik.Reporting.Drawing.Unit.Cm(3.0999999046325684D));
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(15.399896621704102D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox5.Style.Color = System.Drawing.Color.DimGray;
            this.textBox5.Style.Font.Name = "Segoe UI Light";
            this.textBox5.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(11D);
            this.textBox5.Value = "=\"Instances on this device (\" + Fields.InstancesCount + \"):\"";
            // 
            // textBox11
            // 
            this.textBox11.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(3.1000001430511475D), Telerik.Reporting.Drawing.Unit.Cm(0.7585417628288269D));
            this.textBox11.Name = "textBox11";
            this.textBox11.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(6.1999998092651367D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox11.Style.Color = System.Drawing.Color.DimGray;
            this.textBox11.Style.Font.Name = "Segoe UI Light";
            this.textBox11.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(10D);
            this.textBox11.Value = "= Fields.IpAddress";
            // 
            // textBox10
            // 
            this.textBox10.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(3.0999999046325684D), Telerik.Reporting.Drawing.Unit.Cm(1.2965278625488281D));
            this.textBox10.Name = "textBox10";
            this.textBox10.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(6.1999998092651367D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox10.Style.Color = System.Drawing.Color.DimGray;
            this.textBox10.Style.Font.Name = "Segoe UI Light";
            this.textBox10.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(10D);
            this.textBox10.Value = "= Fields.OsName";
            // 
            // textBox9
            // 
            this.textBox9.Bindings.Add(new Telerik.Reporting.Binding("Visible", "= IIF(Len(Fields.OrganizationUnit) <> 0, True, False)"));
            this.textBox9.CanShrink = true;
            this.textBox9.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(3.0999996662139893D), Telerik.Reporting.Drawing.Unit.Cm(2.4000000953674316D));
            this.textBox9.Name = "textBox9";
            this.textBox9.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(12.308959007263184D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox9.Style.Color = System.Drawing.Color.DimGray;
            this.textBox9.Style.Font.Name = "Segoe UI Light";
            this.textBox9.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(10D);
            this.textBox9.Value = "= Fields.OrganizationUnit";
            // 
            // textBox16
            // 
            this.textBox16.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(3.1000001430511475D), Telerik.Reporting.Drawing.Unit.Cm(1.8345141410827637D));
            this.textBox16.Name = "textBox16";
            this.textBox16.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(4.2089581489562988D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox16.Style.Color = System.Drawing.Color.DimGray;
            this.textBox16.Style.Font.Name = "Segoe UI Light";
            this.textBox16.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(10D);
            this.textBox16.Value = "=Fields.TotalFilesCount";
            // 
            // textBox15
            // 
            this.textBox15.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(11.882603645324707D), Telerik.Reporting.Drawing.Unit.Cm(0.758542001247406D));
            this.textBox15.Name = "textBox15";
            this.textBox15.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.5263543128967285D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox15.Style.Color = System.Drawing.Color.DimGray;
            this.textBox15.Style.Font.Name = "Segoe UI Light";
            this.textBox15.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(10D);
            this.textBox15.Value = "=Fields.UnknownFilesCount";
            // 
            // textBox14
            // 
            this.textBox14.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(11.899999618530273D), Telerik.Reporting.Drawing.Unit.Cm(1.2965277433395386D));
            this.textBox14.Name = "textBox14";
            this.textBox14.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.5263543128967285D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox14.Style.Color = System.Drawing.Color.DimGray;
            this.textBox14.Style.Font.Name = "Segoe UI Light";
            this.textBox14.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(10D);
            this.textBox14.Value = "=Fields.MalwareFilesCount";
            // 
            // textBox21
            // 
            this.textBox21.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.026458332315087318D), Telerik.Reporting.Drawing.Unit.Cm(1.8345141410827637D));
            this.textBox21.Name = "textBox21";
            this.textBox21.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox21.Style.Color = System.Drawing.Color.DimGray;
            this.textBox21.Style.Font.Name = "Segoe UI Light";
            this.textBox21.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(10D);
            this.textBox21.Value = "Total Files Scanned:";
            // 
            // textBox20
            // 
            this.textBox20.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(9.40000057220459D), Telerik.Reporting.Drawing.Unit.Cm(0.758542001247406D));
            this.textBox20.Name = "textBox20";
            this.textBox20.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.3999996185302734D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox20.Style.Color = System.Drawing.Color.DimGray;
            this.textBox20.Style.Font.Name = "Segoe UI Light";
            this.textBox20.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(10D);
            this.textBox20.Value = "Unknown Files:";
            // 
            // textBox19
            // 
            this.textBox19.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(9.41739559173584D), Telerik.Reporting.Drawing.Unit.Cm(1.2965277433395386D));
            this.textBox19.Name = "textBox19";
            this.textBox19.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.3999996185302734D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox19.Style.Color = System.Drawing.Color.DimGray;
            this.textBox19.Style.Font.Name = "Segoe UI Light";
            this.textBox19.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(10D);
            this.textBox19.Value = "Malicious Files:";
            // 
            // htmlTextBox1
            // 
            this.htmlTextBox1.KeepTogether = false;
            this.htmlTextBox1.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Inch(0.010416666977107525D), Telerik.Reporting.Drawing.Unit.Inch(1.4566930532455444D));
            this.htmlTextBox1.Name = "htmlTextBox1";
            this.htmlTextBox1.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Inch(6.0561027526855469D), Telerik.Reporting.Drawing.Unit.Inch(0.28822502493858337D));
            this.htmlTextBox1.Style.Color = System.Drawing.Color.DimGray;
            this.htmlTextBox1.Style.Font.Name = "Segoe UI Light";
            this.htmlTextBox1.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.htmlTextBox1.Style.Padding.Bottom = Telerik.Reporting.Drawing.Unit.Point(4D);
            this.htmlTextBox1.Style.Padding.Top = Telerik.Reporting.Drawing.Unit.Point(4D);
            this.htmlTextBox1.Value = "{Fields.InstancesHtml}";
            // 
            // textBox6
            // 
            this.textBox6.Bindings.Add(new Telerik.Reporting.Binding("Visible", "= IIF(Len(Fields.OrganizationUnit) <> 0, True, False)"));
            this.textBox6.CanShrink = true;
            this.textBox6.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0D), Telerik.Reporting.Drawing.Unit.Cm(2.4000000953674316D));
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.995424747467041D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox6.Style.Color = System.Drawing.Color.DimGray;
            this.textBox6.Style.Font.Name = "Segoe UI Light";
            this.textBox6.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(10D);
            this.textBox6.Value = "Organizational Unit:";
            // 
            // pageFooterSection1
            // 
            this.pageFooterSection1.Height = Telerik.Reporting.Drawing.Unit.Cm(0.13229171931743622D);
            this.pageFooterSection1.Name = "pageFooterSection1";
            // 
            // dsModule
            // 
            this.dsModule.DataSource = typeof(APTAT.Core.Reports.ScannedTarget);
            this.dsModule.Name = "dsModule";
            // 
            // Devices
            // 
            this.DataSource = this.dsModule;
            group1.GroupFooter = this.groupFooterSection;
            group1.GroupHeader = this.groupHeaderSection;
            group1.Name = "group";
            this.Groups.AddRange(new Telerik.Reporting.Group[] {
            group1});
            this.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.groupHeaderSection,
            this.groupFooterSection,
            this.pageHeaderSection1,
            this.detail,
            this.pageFooterSection1});
            this.Name = "Devices";
            this.PageSettings.Margins = new Telerik.Reporting.Drawing.MarginsU(Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D), Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D), Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D), Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D));
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            reportParameter1.Name = "IsActiveDirectory";
            reportParameter1.Type = Telerik.Reporting.ReportParameterType.Boolean;
            reportParameter1.Value = "False";
            reportParameter2.AllowBlank = false;
            reportParameter2.Name = "DataSourceBinding";
            reportParameter3.Name = "TargetsCount";
            this.ReportParameters.Add(reportParameter1);
            this.ReportParameters.Add(reportParameter2);
            this.ReportParameters.Add(reportParameter3);
            styleRule1.Selectors.AddRange(new Telerik.Reporting.Drawing.ISelector[] {
            new Telerik.Reporting.Drawing.TypeSelector(typeof(Telerik.Reporting.TextItemBase)),
            new Telerik.Reporting.Drawing.TypeSelector(typeof(Telerik.Reporting.HtmlTextBox))});
            styleRule1.Style.Padding.Left = Telerik.Reporting.Drawing.Unit.Point(2D);
            styleRule1.Style.Padding.Right = Telerik.Reporting.Drawing.Unit.Point(2D);
            this.StyleSheet.AddRange(new Telerik.Reporting.Drawing.StyleRule[] {
            styleRule1});
            this.Width = Telerik.Reporting.Drawing.Unit.Cm(15.426454544067383D);
            this.ItemDataBinding += new System.EventHandler(this.Devices_ItemDataBinding);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        private Telerik.Reporting.PageHeaderSection pageHeaderSection1;
        private Telerik.Reporting.DetailSection detail;
        private Telerik.Reporting.PageFooterSection pageFooterSection1;
        private Telerik.Reporting.Shape shape1;
        private Telerik.Reporting.TextBox textBoxPath;
        private Telerik.Reporting.TextBox textBox2;
        private Telerik.Reporting.TextBox textBox3;
        private Telerik.Reporting.TextBox textBox5;
        private Telerik.Reporting.TextBox textBox11;
        private Telerik.Reporting.TextBox textBox10;
        private Telerik.Reporting.TextBox textBox9;
        private Telerik.Reporting.TextBox textBox16;
        private Telerik.Reporting.TextBox textBox15;
        private Telerik.Reporting.TextBox textBox14;
        private Telerik.Reporting.TextBox textBox21;
        private Telerik.Reporting.TextBox textBox20;
        private Telerik.Reporting.TextBox textBox19;
        private Telerik.Reporting.ObjectDataSource dsModule;
        private Telerik.Reporting.GroupHeaderSection groupHeaderSection;
        private Telerik.Reporting.TextBox textBox44;
        private Telerik.Reporting.GroupFooterSection groupFooterSection;
        private Telerik.Reporting.HtmlTextBox htmlTextBox1;
        private Telerik.Reporting.TextBox textBox6;
    }
}