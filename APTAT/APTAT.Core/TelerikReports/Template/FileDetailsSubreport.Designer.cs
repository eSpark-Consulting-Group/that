namespace APTAT.Core.TelerikReports.Template
{
    partial class FileDetailsSubreport
    {
        #region Component Designer generated code
        /// <summary>
        /// Required method for telerik Reporting designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Telerik.Reporting.Group group1 = new Telerik.Reporting.Group();
            Telerik.Reporting.ReportParameter reportParameter1 = new Telerik.Reporting.ReportParameter();
            Telerik.Reporting.ReportParameter reportParameter2 = new Telerik.Reporting.ReportParameter();
            Telerik.Reporting.ReportParameter reportParameter3 = new Telerik.Reporting.ReportParameter();
            Telerik.Reporting.ReportParameter reportParameter4 = new Telerik.Reporting.ReportParameter();
            Telerik.Reporting.Drawing.StyleRule styleRule1 = new Telerik.Reporting.Drawing.StyleRule();
            this.pageHeaderSection1 = new Telerik.Reporting.PageHeaderSection();
            this.detail = new Telerik.Reporting.DetailSection();
            this.shape1 = new Telerik.Reporting.Shape();
            this.textBoxPath = new Telerik.Reporting.TextBox();
            this.textBox2 = new Telerik.Reporting.TextBox();
            this.textBox3 = new Telerik.Reporting.TextBox();
            this.textBox4 = new Telerik.Reporting.TextBox();
            this.textBox5 = new Telerik.Reporting.TextBox();
            this.textBox6 = new Telerik.Reporting.TextBox();
            this.textBox11 = new Telerik.Reporting.TextBox();
            this.textBox10 = new Telerik.Reporting.TextBox();
            this.textBox9 = new Telerik.Reporting.TextBox();
            this.textBox8 = new Telerik.Reporting.TextBox();
            this.textBox7 = new Telerik.Reporting.TextBox();
            this.textBox16 = new Telerik.Reporting.TextBox();
            this.textBox15 = new Telerik.Reporting.TextBox();
            this.textBox14 = new Telerik.Reporting.TextBox();
            this.textBox13 = new Telerik.Reporting.TextBox();
            this.textBox12 = new Telerik.Reporting.TextBox();
            this.textBox21 = new Telerik.Reporting.TextBox();
            this.textBox20 = new Telerik.Reporting.TextBox();
            this.textBox19 = new Telerik.Reporting.TextBox();
            this.textBox18 = new Telerik.Reporting.TextBox();
            this.textBox17 = new Telerik.Reporting.TextBox();
            this.pageFooterSection1 = new Telerik.Reporting.PageFooterSection();
            this.groupHeaderSection = new Telerik.Reporting.GroupHeaderSection();
            this.groupFooterSection = new Telerik.Reporting.GroupFooterSection();
            this.textBox1 = new Telerik.Reporting.TextBox();
            this.dsModule = new Telerik.Reporting.ObjectDataSource();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // pageHeaderSection1
            // 
            this.pageHeaderSection1.Height = Telerik.Reporting.Drawing.Unit.Cm(0.13229167461395264D);
            this.pageHeaderSection1.Name = "pageHeaderSection1";
            // 
            // detail
            // 
            this.detail.Height = Telerik.Reporting.Drawing.Unit.Cm(3.4376914501190186D);
            this.detail.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.shape1,
            this.textBoxPath,
            this.textBox2,
            this.textBox3,
            this.textBox4,
            this.textBox5,
            this.textBox6,
            this.textBox11,
            this.textBox10,
            this.textBox9,
            this.textBox8,
            this.textBox7,
            this.textBox16,
            this.textBox15,
            this.textBox14,
            this.textBox13,
            this.textBox12,
            this.textBox21,
            this.textBox20,
            this.textBox19,
            this.textBox18,
            this.textBox17});
            this.detail.Name = "detail";
            // 
            // shape1
            // 
            this.shape1.Bindings.Add(new Telerik.Reporting.Binding("Style.BackgroundColor", "= Parameters.MarkColor.Value"));
            this.shape1.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.20000003278255463D), Telerik.Reporting.Drawing.Unit.Cm(0.23769183456897736D));
            this.shape1.Name = "shape1";
            this.shape1.ShapeType = new Telerik.Reporting.Drawing.Shapes.EllipseShape(0D);
            this.shape1.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(0.20000000298023224D), Telerik.Reporting.Drawing.Unit.Cm(0.20000000298023224D));
            this.shape1.Style.BackgroundColor = System.Drawing.Color.DarkSeaGreen;
            this.shape1.Style.BorderColor.Default = System.Drawing.Color.Transparent;
            this.shape1.Style.BorderWidth.Default = Telerik.Reporting.Drawing.Unit.Point(0D);
            this.shape1.Style.Color = System.Drawing.Color.Transparent;
            // 
            // textBoxPath
            // 
            this.textBoxPath.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.47916698455810547D), Telerik.Reporting.Drawing.Unit.Cm(0.027274662628769875D));
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(15.399996757507324D), Telerik.Reporting.Drawing.Unit.Cm(0.49999994039535522D));
            this.textBoxPath.Style.Font.Name = "Segoe UI Light";
            this.textBoxPath.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(11D);
            this.textBoxPath.Value = "= Fields.Path";
            // 
            // textBox2
            // 
            this.textBox2.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.47916698455810547D), Telerik.Reporting.Drawing.Unit.Cm(0.76810795068740845D));
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.2483415603637695D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox2.Style.Color = System.Drawing.Color.DimGray;
            this.textBox2.Style.Font.Name = "Segoe UI Light";
            this.textBox2.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox2.Value = "Size:";
            // 
            // textBox3
            // 
            this.textBox3.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.47916698455810547D), Telerik.Reporting.Drawing.Unit.Cm(1.2708162069320679D));
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.2483415603637695D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox3.Style.Color = System.Drawing.Color.DimGray;
            this.textBox3.Style.Font.Name = "Segoe UI Light";
            this.textBox3.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox3.Value = "Created:";
            // 
            // textBox4
            // 
            this.textBox4.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.47916698455810547D), Telerik.Reporting.Drawing.Unit.Cm(1.8264411687850952D));
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.2483415603637695D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox4.Style.Color = System.Drawing.Color.DimGray;
            this.textBox4.Style.Font.Name = "Segoe UI Light";
            this.textBox4.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox4.Value = "Modified:";
            // 
            // textBox5
            // 
            this.textBox5.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.47916698455810547D), Telerik.Reporting.Drawing.Unit.Cm(2.3820667266845703D));
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.2483415603637695D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox5.Style.Color = System.Drawing.Color.DimGray;
            this.textBox5.Style.Font.Name = "Segoe UI Light";
            this.textBox5.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox5.Value = "Last Accessed:";
            // 
            // textBox6
            // 
            this.textBox6.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.47916698455810547D), Telerik.Reporting.Drawing.Unit.Cm(2.9376914501190186D));
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.2483415603637695D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox6.Style.Color = System.Drawing.Color.DimGray;
            this.textBox6.Style.Font.Name = "Segoe UI Light";
            this.textBox6.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox6.Value = "File Age::";
            // 
            // textBox11
            // 
            this.textBox11.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(2.7677087783813477D), Telerik.Reporting.Drawing.Unit.Cm(0.76810795068740845D));
            this.textBox11.Name = "textBox11";
            this.textBox11.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.2320911884307861D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox11.Style.Color = System.Drawing.Color.DimGray;
            this.textBox11.Style.Font.Name = "Segoe UI Light";
            this.textBox11.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox11.Value = "=IIF(Fields.FileSizeFormatted <> \"\", Fields.FileSizeFormatted, \"N/A\")";
            // 
            // textBox10
            // 
            this.textBox10.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(2.7677087783813477D), Telerik.Reporting.Drawing.Unit.Cm(1.2708162069320679D));
            this.textBox10.Name = "textBox10";
            this.textBox10.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.2320911884307861D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox10.Style.Color = System.Drawing.Color.DimGray;
            this.textBox10.Style.Font.Name = "Segoe UI Light";
            this.textBox10.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox10.Value = "= Fields.Created";
            // 
            // textBox9
            // 
            this.textBox9.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(2.7677087783813477D), Telerik.Reporting.Drawing.Unit.Cm(1.8264411687850952D));
            this.textBox9.Name = "textBox9";
            this.textBox9.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.2320911884307861D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox9.Style.Color = System.Drawing.Color.DimGray;
            this.textBox9.Style.Font.Name = "Segoe UI Light";
            this.textBox9.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox9.Value = "= Fields.Modified";
            // 
            // textBox8
            // 
            this.textBox8.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(2.7677087783813477D), Telerik.Reporting.Drawing.Unit.Cm(2.3820667266845703D));
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.2320911884307861D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox8.Style.Color = System.Drawing.Color.DimGray;
            this.textBox8.Style.Font.Name = "Segoe UI Light";
            this.textBox8.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox8.Value = "= Fields.Accessed";
            // 
            // textBox7
            // 
            this.textBox7.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(2.7677087783813477D), Telerik.Reporting.Drawing.Unit.Cm(2.9376914501190186D));
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.2320911884307861D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox7.Style.Color = System.Drawing.Color.DimGray;
            this.textBox7.Style.Font.Name = "Segoe UI Light";
            this.textBox7.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox7.Value = "= IIF(Fields.FileAge <>\"\", Fields.FileAge, \"N/A\")";
            // 
            // textBox16
            // 
            this.textBox16.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(8.010833740234375D), Telerik.Reporting.Drawing.Unit.Cm(0.76810795068740845D));
            this.textBox16.Name = "textBox16";
            this.textBox16.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(7.8683333396911621D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox16.Style.Color = System.Drawing.Color.DimGray;
            this.textBox16.Style.Font.Name = "Segoe UI Light";
            this.textBox16.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox16.Value = "=IIf(Fields.Signer <> \"\", Fields.Signer,  \"N/A\")\r\n";
            // 
            // textBox15
            // 
            this.textBox15.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(8.010833740234375D), Telerik.Reporting.Drawing.Unit.Cm(1.2708162069320679D));
            this.textBox15.Name = "textBox15";
            this.textBox15.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(7.8683333396911621D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox15.Style.Color = System.Drawing.Color.DimGray;
            this.textBox15.Style.Font.Name = "Segoe UI Light";
            this.textBox15.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox15.Value = "=IIF(Fields.Version <> \"\", Fields.Version, \"N/A\")";
            // 
            // textBox14
            // 
            this.textBox14.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(8.010833740234375D), Telerik.Reporting.Drawing.Unit.Cm(1.8264411687850952D));
            this.textBox14.Name = "textBox14";
            this.textBox14.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(7.8683333396911621D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox14.Style.Color = System.Drawing.Color.DimGray;
            this.textBox14.Style.Font.Name = "Segoe UI Light";
            this.textBox14.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox14.Value = "=IIF(Fields.Sha1 <> \"\", Fields.Sha1, \"N/A\")";
            // 
            // textBox13
            // 
            this.textBox13.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(8.010833740234375D), Telerik.Reporting.Drawing.Unit.Cm(2.3820667266845703D));
            this.textBox13.Name = "textBox13";
            this.textBox13.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(7.8683333396911621D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox13.Style.Color = System.Drawing.Color.DimGray;
            this.textBox13.Style.Font.Name = "Segoe UI Light";
            this.textBox13.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox13.Value = "=IIF(Fields.Product <> \"\", Fields.Product, \"N/A\")";
            // 
            // textBox12
            // 
            this.textBox12.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(8.010833740234375D), Telerik.Reporting.Drawing.Unit.Cm(2.9376914501190186D));
            this.textBox12.Name = "textBox12";
            this.textBox12.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(7.8683333396911621D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox12.Style.Color = System.Drawing.Color.DimGray;
            this.textBox12.Style.Font.Name = "Segoe UI Light";
            this.textBox12.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox12.Value = "=IIF(Fields.Copyright <> \"\", Fields.Copyright, \"N/A\")";
            // 
            // textBox21
            // 
            this.textBox21.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(6D), Telerik.Reporting.Drawing.Unit.Cm(0.76810795068740845D));
            this.textBox21.Name = "textBox21";
            this.textBox21.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.9999997615814209D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox21.Style.Color = System.Drawing.Color.DimGray;
            this.textBox21.Style.Font.Name = "Segoe UI Light";
            this.textBox21.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox21.Value = "Signer:";
            // 
            // textBox20
            // 
            this.textBox20.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(6D), Telerik.Reporting.Drawing.Unit.Cm(1.2708162069320679D));
            this.textBox20.Name = "textBox20";
            this.textBox20.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.9999997615814209D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox20.Style.Color = System.Drawing.Color.DimGray;
            this.textBox20.Style.Font.Name = "Segoe UI Light";
            this.textBox20.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox20.Value = "Version:";
            // 
            // textBox19
            // 
            this.textBox19.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(6D), Telerik.Reporting.Drawing.Unit.Cm(1.8264411687850952D));
            this.textBox19.Name = "textBox19";
            this.textBox19.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.9999997615814209D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox19.Style.Color = System.Drawing.Color.DimGray;
            this.textBox19.Style.Font.Name = "Segoe UI Light";
            this.textBox19.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox19.Value = "SHA1:";
            // 
            // textBox18
            // 
            this.textBox18.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(6D), Telerik.Reporting.Drawing.Unit.Cm(2.3820667266845703D));
            this.textBox18.Name = "textBox18";
            this.textBox18.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.9997994899749756D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox18.Style.Color = System.Drawing.Color.DimGray;
            this.textBox18.Style.Font.Name = "Segoe UI Light";
            this.textBox18.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox18.Value = "Product:";
            // 
            // textBox17
            // 
            this.textBox17.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(6D), Telerik.Reporting.Drawing.Unit.Cm(2.9376914501190186D));
            this.textBox17.Name = "textBox17";
            this.textBox17.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.9999997615814209D), Telerik.Reporting.Drawing.Unit.Cm(0.4999997615814209D));
            this.textBox17.Style.Color = System.Drawing.Color.DimGray;
            this.textBox17.Style.Font.Name = "Segoe UI Light";
            this.textBox17.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox17.Value = "Copyright:";
            // 
            // pageFooterSection1
            // 
            this.pageFooterSection1.Height = Telerik.Reporting.Drawing.Unit.Cm(0.13229171931743622D);
            this.pageFooterSection1.Name = "pageFooterSection1";
            // 
            // groupHeaderSection
            // 
            this.groupHeaderSection.Height = Telerik.Reporting.Drawing.Unit.Cm(1.1675081253051758D);
            this.groupHeaderSection.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.textBox1});
            this.groupHeaderSection.Name = "groupHeaderSection";
            // 
            // groupFooterSection
            // 
            this.groupFooterSection.Height = Telerik.Reporting.Drawing.Unit.Cm(0.13229171931743622D);
            this.groupFooterSection.Name = "groupFooterSection";
            // 
            // textBox1
            // 
            this.textBox1.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(6.3081579959600731E-09D), Telerik.Reporting.Drawing.Unit.Cm(0.067708350718021393D));
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(15.399996757507324D), Telerik.Reporting.Drawing.Unit.Cm(0.93001633882522583D));
            this.textBox1.Style.BackgroundColor = System.Drawing.Color.Empty;
            this.textBox1.Style.Color = System.Drawing.Color.Gray;
            this.textBox1.Style.Font.Name = "Segoe UI Light";
            this.textBox1.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(18D);
            this.textBox1.Value = "= Parameters.Title.Value";
            // 
            // dsModule
            // 
            this.dsModule.DataSource = typeof(APTAT.Core.Reports.DetailedBadFile);
            this.dsModule.Name = "dsModule";
            // 
            // FileDetailsSubreport
            // 
            this.DataSource = this.dsModule;
            group1.GroupFooter = this.groupFooterSection;
            group1.GroupHeader = this.groupHeaderSection;
            group1.Name = "group";
            this.Groups.AddRange(new Telerik.Reporting.Group[] {
            group1});
            this.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.pageHeaderSection1,
            this.detail,
            this.pageFooterSection1,
            this.groupHeaderSection,
            this.groupFooterSection});
            this.Name = "FileDetailsSubreport";
            this.PageSettings.Margins = new Telerik.Reporting.Drawing.MarginsU(Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D), Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D), Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D), Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D));
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            reportParameter1.Name = "Title";
            reportParameter1.Value = "Files";
            reportParameter2.Name = "Count";
            reportParameter2.Type = Telerik.Reporting.ReportParameterType.Integer;
            reportParameter2.Value = "0";
            reportParameter3.Name = "MarkColor";
            reportParameter3.Value = "Red";
            reportParameter4.AllowBlank = false;
            reportParameter4.Name = "DataSourceBinding";
            this.ReportParameters.Add(reportParameter1);
            this.ReportParameters.Add(reportParameter2);
            this.ReportParameters.Add(reportParameter3);
            this.ReportParameters.Add(reportParameter4);
            styleRule1.Selectors.AddRange(new Telerik.Reporting.Drawing.ISelector[] {
            new Telerik.Reporting.Drawing.TypeSelector(typeof(Telerik.Reporting.TextItemBase)),
            new Telerik.Reporting.Drawing.TypeSelector(typeof(Telerik.Reporting.HtmlTextBox))});
            styleRule1.Style.Padding.Left = Telerik.Reporting.Drawing.Unit.Point(2D);
            styleRule1.Style.Padding.Right = Telerik.Reporting.Drawing.Unit.Point(2D);
            this.StyleSheet.AddRange(new Telerik.Reporting.Drawing.StyleRule[] {
            styleRule1});
            this.Width = Telerik.Reporting.Drawing.Unit.Cm(15.879165649414063D);
            this.ItemDataBinding += new System.EventHandler(this.FileDetailsSubreport_ItemDataBinding);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        private Telerik.Reporting.PageHeaderSection pageHeaderSection1;
        private Telerik.Reporting.DetailSection detail;
        private Telerik.Reporting.PageFooterSection pageFooterSection1;
        private Telerik.Reporting.ObjectDataSource dsModule;
        private Telerik.Reporting.Shape shape1;
        private Telerik.Reporting.TextBox textBoxPath;
        private Telerik.Reporting.TextBox textBox2;
        private Telerik.Reporting.TextBox textBox3;
        private Telerik.Reporting.TextBox textBox4;
        private Telerik.Reporting.TextBox textBox5;
        private Telerik.Reporting.TextBox textBox6;
        private Telerik.Reporting.TextBox textBox11;
        private Telerik.Reporting.TextBox textBox10;
        private Telerik.Reporting.TextBox textBox9;
        private Telerik.Reporting.TextBox textBox8;
        private Telerik.Reporting.TextBox textBox7;
        private Telerik.Reporting.TextBox textBox16;
        private Telerik.Reporting.TextBox textBox15;
        private Telerik.Reporting.TextBox textBox14;
        private Telerik.Reporting.TextBox textBox13;
        private Telerik.Reporting.TextBox textBox12;
        private Telerik.Reporting.TextBox textBox21;
        private Telerik.Reporting.TextBox textBox20;
        private Telerik.Reporting.TextBox textBox19;
        private Telerik.Reporting.TextBox textBox18;
        private Telerik.Reporting.TextBox textBox17;
        private Telerik.Reporting.GroupHeaderSection groupHeaderSection;
        private Telerik.Reporting.TextBox textBox1;
        private Telerik.Reporting.GroupFooterSection groupFooterSection;
    }
}