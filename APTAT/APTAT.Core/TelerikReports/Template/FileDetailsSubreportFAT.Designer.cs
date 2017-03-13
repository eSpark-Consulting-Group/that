namespace APTAT.Core.TelerikReports.Template
{
    partial class FileDetailsSubreportFAT
    {
        #region Component Designer generated code
        /// <summary>
        /// Required method for telerik Reporting designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Telerik.Reporting.TableGroup tableGroup1 = new Telerik.Reporting.TableGroup();
            Telerik.Reporting.TableGroup tableGroup2 = new Telerik.Reporting.TableGroup();
            Telerik.Reporting.TableGroup tableGroup3 = new Telerik.Reporting.TableGroup();
            Telerik.Reporting.TableGroup tableGroup4 = new Telerik.Reporting.TableGroup();
            Telerik.Reporting.TableGroup tableGroup5 = new Telerik.Reporting.TableGroup();
            Telerik.Reporting.TableGroup tableGroup6 = new Telerik.Reporting.TableGroup();
            Telerik.Reporting.TableGroup tableGroup7 = new Telerik.Reporting.TableGroup();
            Telerik.Reporting.TableGroup tableGroup8 = new Telerik.Reporting.TableGroup();
            Telerik.Reporting.TableGroup tableGroup9 = new Telerik.Reporting.TableGroup();
            Telerik.Reporting.TableGroup tableGroup10 = new Telerik.Reporting.TableGroup();
            Telerik.Reporting.Group group1 = new Telerik.Reporting.Group();
            Telerik.Reporting.ReportParameter reportParameter1 = new Telerik.Reporting.ReportParameter();
            Telerik.Reporting.ReportParameter reportParameter2 = new Telerik.Reporting.ReportParameter();
            Telerik.Reporting.ReportParameter reportParameter3 = new Telerik.Reporting.ReportParameter();
            Telerik.Reporting.Drawing.StyleRule styleRule1 = new Telerik.Reporting.Drawing.StyleRule();
            this.groupFooterSection = new Telerik.Reporting.GroupFooterSection();
            this.groupHeaderSection = new Telerik.Reporting.GroupHeaderSection();
            this.textBox1 = new Telerik.Reporting.TextBox();
            this.pageHeaderSection1 = new Telerik.Reporting.PageHeaderSection();
            this.detail = new Telerik.Reporting.DetailSection();
            this.shape1 = new Telerik.Reporting.Shape();
            this.textBoxPath = new Telerik.Reporting.TextBox();
            this.list1 = new Telerik.Reporting.List();
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
            this.textBox22 = new Telerik.Reporting.TextBox();
            this.textBox21 = new Telerik.Reporting.TextBox();
            this.textBox20 = new Telerik.Reporting.TextBox();
            this.textBox19 = new Telerik.Reporting.TextBox();
            this.textBox18 = new Telerik.Reporting.TextBox();
            this.textBox17 = new Telerik.Reporting.TextBox();
            this.textBox15 = new Telerik.Reporting.TextBox();
            this.textBox14 = new Telerik.Reporting.TextBox();
            this.textBox16 = new Telerik.Reporting.TextBox();
            this.textBox12 = new Telerik.Reporting.TextBox();
            this.pageFooterSection1 = new Telerik.Reporting.PageFooterSection();
            this.dsModule = new Telerik.Reporting.ObjectDataSource();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // groupFooterSection
            // 
            this.groupFooterSection.Height = Telerik.Reporting.Drawing.Unit.Cm(0.13229171931743622D);
            this.groupFooterSection.Name = "groupFooterSection";
            // 
            // groupHeaderSection
            // 
            this.groupHeaderSection.Height = Telerik.Reporting.Drawing.Unit.Cm(1.2677083015441895D);
            this.groupHeaderSection.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.textBox1});
            this.groupHeaderSection.Name = "groupHeaderSection";
            // 
            // textBox1
            // 
            this.textBox1.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(6.3081579959600731E-09D), Telerik.Reporting.Drawing.Unit.Cm(0.16770829260349274D));
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(15.399996757507324D), Telerik.Reporting.Drawing.Unit.Cm(0.93001633882522583D));
            this.textBox1.Style.BackgroundColor = System.Drawing.Color.Empty;
            this.textBox1.Style.Color = System.Drawing.Color.Gray;
            this.textBox1.Style.Font.Name = "Segoe UI Light";
            this.textBox1.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(18D);
            this.textBox1.Value = "= Parameters.Title.Value";
            // 
            // pageHeaderSection1
            // 
            this.pageHeaderSection1.Height = Telerik.Reporting.Drawing.Unit.Cm(0.13229167461395264D);
            this.pageHeaderSection1.Name = "pageHeaderSection1";
            // 
            // detail
            // 
            this.detail.Height = Telerik.Reporting.Drawing.Unit.Cm(3.4999997615814209D);
            this.detail.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.shape1,
            this.textBoxPath,
            this.list1});
            this.detail.Name = "detail";
            // 
            // shape1
            // 
            this.shape1.Bindings.Add(new Telerik.Reporting.Binding("Style.BackgroundColor", "= Parameters.MarkColor.Value"));
            this.shape1.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.20000003278255463D), Telerik.Reporting.Drawing.Unit.Cm(0.26770833134651184D));
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
            this.textBoxPath.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.47916698455810547D), Telerik.Reporting.Drawing.Unit.Cm(0.057291090488433838D));
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(15.399996757507324D), Telerik.Reporting.Drawing.Unit.Cm(0.49999994039535522D));
            this.textBoxPath.Style.Font.Name = "Segoe UI";
            this.textBoxPath.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(11D);
            this.textBoxPath.Value = "= Fields.Path";
            // 
            // list1
            // 
            this.list1.Bindings.Add(new Telerik.Reporting.Binding("DataSource", "= ReportItem.DataObject"));
            this.list1.Body.Columns.Add(new Telerik.Reporting.TableBodyColumn(Telerik.Reporting.Drawing.Unit.Cm(1.9999973773956299D)));
            this.list1.Body.Columns.Add(new Telerik.Reporting.TableBodyColumn(Telerik.Reporting.Drawing.Unit.Cm(3.2999987602233887D)));
            this.list1.Body.Columns.Add(new Telerik.Reporting.TableBodyColumn(Telerik.Reporting.Drawing.Unit.Cm(2.0000007152557373D)));
            this.list1.Body.Columns.Add(new Telerik.Reporting.TableBodyColumn(Telerik.Reporting.Drawing.Unit.Cm(8.00000286102295D)));
            this.list1.Body.Rows.Add(new Telerik.Reporting.TableBodyRow(Telerik.Reporting.Drawing.Unit.Cm(0.5D)));
            this.list1.Body.Rows.Add(new Telerik.Reporting.TableBodyRow(Telerik.Reporting.Drawing.Unit.Cm(0.5D)));
            this.list1.Body.Rows.Add(new Telerik.Reporting.TableBodyRow(Telerik.Reporting.Drawing.Unit.Cm(0.5D)));
            this.list1.Body.Rows.Add(new Telerik.Reporting.TableBodyRow(Telerik.Reporting.Drawing.Unit.Cm(0.5D)));
            this.list1.Body.Rows.Add(new Telerik.Reporting.TableBodyRow(Telerik.Reporting.Drawing.Unit.Cm(0.5D)));
            this.list1.Body.SetCellContent(0, 0, this.textBox2);
            this.list1.Body.SetCellContent(1, 0, this.textBox3);
            this.list1.Body.SetCellContent(2, 0, this.textBox4);
            this.list1.Body.SetCellContent(3, 0, this.textBox5);
            this.list1.Body.SetCellContent(4, 0, this.textBox6);
            this.list1.Body.SetCellContent(0, 1, this.textBox11);
            this.list1.Body.SetCellContent(1, 1, this.textBox10);
            this.list1.Body.SetCellContent(2, 1, this.textBox9);
            this.list1.Body.SetCellContent(3, 1, this.textBox8);
            this.list1.Body.SetCellContent(4, 1, this.textBox7);
            this.list1.Body.SetCellContent(0, 3, this.textBox22);
            this.list1.Body.SetCellContent(0, 2, this.textBox21);
            this.list1.Body.SetCellContent(1, 2, this.textBox20);
            this.list1.Body.SetCellContent(2, 2, this.textBox19);
            this.list1.Body.SetCellContent(3, 2, this.textBox18);
            this.list1.Body.SetCellContent(4, 2, this.textBox17);
            this.list1.Body.SetCellContent(1, 3, this.textBox15);
            this.list1.Body.SetCellContent(2, 3, this.textBox14);
            this.list1.Body.SetCellContent(3, 3, this.textBox16);
            this.list1.Body.SetCellContent(4, 3, this.textBox12);
            tableGroup1.Name = "group1";
            tableGroup2.Name = "group10";
            tableGroup3.Name = "ColumnGroup";
            tableGroup4.Name = "group8";
            this.list1.ColumnGroups.Add(tableGroup1);
            this.list1.ColumnGroups.Add(tableGroup2);
            this.list1.ColumnGroups.Add(tableGroup3);
            this.list1.ColumnGroups.Add(tableGroup4);
            this.list1.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.textBox2,
            this.textBox11,
            this.textBox21,
            this.textBox22,
            this.textBox3,
            this.textBox10,
            this.textBox20,
            this.textBox15,
            this.textBox4,
            this.textBox9,
            this.textBox19,
            this.textBox14,
            this.textBox5,
            this.textBox8,
            this.textBox18,
            this.textBox16,
            this.textBox6,
            this.textBox7,
            this.textBox17,
            this.textBox12});
            this.list1.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.5D), Telerik.Reporting.Drawing.Unit.Cm(0.79999995231628418D));
            this.list1.Name = "list1";
            tableGroup6.Name = "group7";
            tableGroup7.Name = "group6";
            tableGroup8.Name = "group5";
            tableGroup9.Name = "group4";
            tableGroup10.Name = "group3";
            tableGroup5.ChildGroups.Add(tableGroup6);
            tableGroup5.ChildGroups.Add(tableGroup7);
            tableGroup5.ChildGroups.Add(tableGroup8);
            tableGroup5.ChildGroups.Add(tableGroup9);
            tableGroup5.ChildGroups.Add(tableGroup10);
            tableGroup5.Groupings.Add(new Telerik.Reporting.Grouping(null));
            tableGroup5.Name = "DetailGroup";
            this.list1.RowGroups.Add(tableGroup5);
            this.list1.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(15.300000190734863D), Telerik.Reporting.Drawing.Unit.Cm(2.5D));
            // 
            // textBox2
            // 
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.9999996423721314D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox2.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251)))));
            this.textBox2.Style.Color = System.Drawing.Color.DimGray;
            this.textBox2.Style.Font.Name = "Segoe UI";
            this.textBox2.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox2.Value = "Size:";
            // 
            // textBox3
            // 
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.9999996423721314D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox3.Style.BackgroundColor = System.Drawing.Color.White;
            this.textBox3.Style.Color = System.Drawing.Color.DimGray;
            this.textBox3.Style.Font.Name = "Segoe UI";
            this.textBox3.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox3.Value = "Created:";
            // 
            // textBox4
            // 
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.9999996423721314D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox4.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251)))));
            this.textBox4.Style.Color = System.Drawing.Color.DimGray;
            this.textBox4.Style.Font.Name = "Segoe UI";
            this.textBox4.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox4.Value = "Modified:";
            // 
            // textBox5
            // 
            this.textBox5.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.47916698455810547D), Telerik.Reporting.Drawing.Unit.Cm(2.4120831489562988D));
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.9999996423721314D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox5.Style.Color = System.Drawing.Color.DimGray;
            this.textBox5.Style.Font.Name = "Segoe UI";
            this.textBox5.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox5.Value = "Last Accessed:";
            // 
            // textBox6
            // 
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.9999996423721314D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox6.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251)))));
            this.textBox6.Style.Color = System.Drawing.Color.DimGray;
            this.textBox6.Style.Font.Name = "Segoe UI";
            this.textBox6.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox6.Value = "File Age:";
            // 
            // textBox11
            // 
            this.textBox11.Name = "textBox11";
            this.textBox11.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.2999997138977051D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox11.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251)))));
            this.textBox11.Style.Color = System.Drawing.Color.DimGray;
            this.textBox11.Style.Font.Name = "Segoe UI";
            this.textBox11.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox11.Value = "=IIF(Fields.FileSizeFormatted <> \"\", Fields.FileSizeFormatted, \"N/A\")";
            // 
            // textBox10
            // 
            this.textBox10.Name = "textBox10";
            this.textBox10.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.2999997138977051D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox10.Style.BackgroundColor = System.Drawing.Color.White;
            this.textBox10.Style.Color = System.Drawing.Color.DimGray;
            this.textBox10.Style.Font.Name = "Segoe UI";
            this.textBox10.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox10.Value = "= Fields.Created";
            // 
            // textBox9
            // 
            this.textBox9.Name = "textBox9";
            this.textBox9.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.2999997138977051D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox9.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251)))));
            this.textBox9.Style.Color = System.Drawing.Color.DimGray;
            this.textBox9.Style.Font.Name = "Segoe UI";
            this.textBox9.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox9.Value = "= Fields.Modified";
            // 
            // textBox8
            // 
            this.textBox8.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(2.7677087783813477D), Telerik.Reporting.Drawing.Unit.Cm(2.4120831489562988D));
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.2999997138977051D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox8.Style.Color = System.Drawing.Color.DimGray;
            this.textBox8.Style.Font.Name = "Segoe UI";
            this.textBox8.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox8.Value = "= Fields.Accessed";
            // 
            // textBox7
            // 
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.2999997138977051D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox7.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251)))));
            this.textBox7.Style.Color = System.Drawing.Color.DimGray;
            this.textBox7.Style.Font.Name = "Segoe UI";
            this.textBox7.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox7.Value = "= IIF(Fields.FileAge <>\"\", Fields.FileAge, \"N/A\")";
            // 
            // textBox22
            // 
            this.textBox22.Name = "textBox22";
            this.textBox22.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(8.0000009536743164D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox22.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251)))));
            this.textBox22.Style.Color = System.Drawing.Color.DimGray;
            this.textBox22.Style.Font.Name = "Segoe UI";
            this.textBox22.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox22.Value = "=IIf(Fields.Signer <> \"\", Fields.Signer,  \"N/A\")\r\n";
            // 
            // textBox21
            // 
            this.textBox21.Name = "textBox21";
            this.textBox21.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2D), Telerik.Reporting.Drawing.Unit.Cm(0.49999994039535522D));
            this.textBox21.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251)))));
            this.textBox21.Style.Color = System.Drawing.Color.DimGray;
            this.textBox21.Style.Font.Name = "Segoe UI";
            this.textBox21.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox21.Value = "Signer:";
            // 
            // textBox20
            // 
            this.textBox20.Name = "textBox20";
            this.textBox20.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2D), Telerik.Reporting.Drawing.Unit.Cm(0.49999994039535522D));
            this.textBox20.Style.BackgroundColor = System.Drawing.Color.White;
            this.textBox20.Style.Color = System.Drawing.Color.DimGray;
            this.textBox20.Style.Font.Name = "Segoe UI";
            this.textBox20.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox20.Value = "Version:";
            // 
            // textBox19
            // 
            this.textBox19.Name = "textBox19";
            this.textBox19.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2D), Telerik.Reporting.Drawing.Unit.Cm(0.49999994039535522D));
            this.textBox19.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251)))));
            this.textBox19.Style.Color = System.Drawing.Color.DimGray;
            this.textBox19.Style.Font.Name = "Segoe UI";
            this.textBox19.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox19.Value = "SHA1:";
            // 
            // textBox18
            // 
            this.textBox18.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(6D), Telerik.Reporting.Drawing.Unit.Cm(2.4120831489562988D));
            this.textBox18.Name = "textBox18";
            this.textBox18.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2D), Telerik.Reporting.Drawing.Unit.Cm(0.49999994039535522D));
            this.textBox18.Style.Color = System.Drawing.Color.DimGray;
            this.textBox18.Style.Font.Name = "Segoe UI";
            this.textBox18.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox18.Value = "Product:";
            // 
            // textBox17
            // 
            this.textBox17.Name = "textBox17";
            this.textBox17.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2D), Telerik.Reporting.Drawing.Unit.Cm(0.49999994039535522D));
            this.textBox17.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251)))));
            this.textBox17.Style.Color = System.Drawing.Color.DimGray;
            this.textBox17.Style.Font.Name = "Segoe UI";
            this.textBox17.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox17.Value = "Copyright:";
            // 
            // textBox15
            // 
            this.textBox15.Name = "textBox15";
            this.textBox15.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(8.0000009536743164D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox15.Style.BackgroundColor = System.Drawing.Color.White;
            this.textBox15.Style.Color = System.Drawing.Color.DimGray;
            this.textBox15.Style.Font.Name = "Segoe UI";
            this.textBox15.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox15.Value = "=IIF(Fields.Version <> \"\", Fields.Version, \"N/A\")";
            // 
            // textBox14
            // 
            this.textBox14.Name = "textBox14";
            this.textBox14.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(8.0000009536743164D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox14.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251)))));
            this.textBox14.Style.Color = System.Drawing.Color.DimGray;
            this.textBox14.Style.Font.Name = "Segoe UI";
            this.textBox14.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox14.Value = "=IIF(Fields.Sha1 <> \"\", Fields.Sha1, \"N/A\")";
            // 
            // textBox16
            // 
            this.textBox16.Name = "textBox16";
            this.textBox16.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(8.0000009536743164D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox16.Style.Color = System.Drawing.Color.DimGray;
            this.textBox16.Style.Font.Name = "Segoe UI";
            this.textBox16.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox16.Value = "=IIF(Fields.Product <> \"\", Fields.Product, \"N/A\")";
            // 
            // textBox12
            // 
            this.textBox12.Name = "textBox12";
            this.textBox12.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(8.0000009536743164D), Telerik.Reporting.Drawing.Unit.Cm(0.5D));
            this.textBox12.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251)))));
            this.textBox12.Style.Color = System.Drawing.Color.DimGray;
            this.textBox12.Style.Font.Name = "Segoe UI";
            this.textBox12.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(8D);
            this.textBox12.Value = "=IIF(Fields.Copyright <> \"\", Fields.Copyright, \"N/A\")";
            // 
            // pageFooterSection1
            // 
            this.pageFooterSection1.Height = Telerik.Reporting.Drawing.Unit.Cm(0.13229171931743622D);
            this.pageFooterSection1.Name = "pageFooterSection1";
            // 
            // dsModule
            // 
            this.dsModule.DataSource = typeof(APTAT.Core.Reports.DetailedBadFile);
            this.dsModule.Name = "dsModule";
            // 
            // FileDetailsValkyrieSubreport
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
            this.Name = "FileDetailsSubreport";
            this.PageSettings.Margins = new Telerik.Reporting.Drawing.MarginsU(Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D), Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D), Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D), Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D));
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            reportParameter1.Name = "Title";
            reportParameter1.Text = "Not set";
            reportParameter1.Value = "Files";
            reportParameter2.Name = "MarkColor";
            reportParameter2.Value = "Red";
            reportParameter3.Name = "DataSourceBinding";
            reportParameter3.Text = "none";
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
            this.Width = Telerik.Reporting.Drawing.Unit.Cm(15.919999122619629D);
            this.ItemDataBinding += new System.EventHandler(this.FileDetailsValkyrieSubreport_ItemDataBinding);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        private Telerik.Reporting.PageHeaderSection pageHeaderSection1;
        private Telerik.Reporting.DetailSection detail;
        private Telerik.Reporting.PageFooterSection pageFooterSection1;
        private Telerik.Reporting.ObjectDataSource dsModule;
        private Telerik.Reporting.Shape shape1;
        private Telerik.Reporting.TextBox textBoxPath;
        private Telerik.Reporting.GroupHeaderSection groupHeaderSection;
        private Telerik.Reporting.GroupFooterSection groupFooterSection;
        private Telerik.Reporting.TextBox textBox1;
        private Telerik.Reporting.List list1;
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
        private Telerik.Reporting.TextBox textBox22;
        private Telerik.Reporting.TextBox textBox21;
        private Telerik.Reporting.TextBox textBox20;
        private Telerik.Reporting.TextBox textBox19;
        private Telerik.Reporting.TextBox textBox18;
        private Telerik.Reporting.TextBox textBox17;
        private Telerik.Reporting.TextBox textBox15;
        private Telerik.Reporting.TextBox textBox14;
        private Telerik.Reporting.TextBox textBox16;
        private Telerik.Reporting.TextBox textBox12;
    }
}