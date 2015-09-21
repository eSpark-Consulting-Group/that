namespace APTAT.Valkyrie.ClientTest
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button2 = new System.Windows.Forms.Button();
            this.sHA1DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.filePathDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isWhiteListedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.lastAnalysisDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.verdictDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isSkippedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.skipReasonDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.malwareProbabilityDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.firstAnalysisDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.returnCodeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 27);
            this.button1.TabIndex = 0;
            this.button1.Text = "Ask Valkyrie";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.sHA1DataGridViewTextBoxColumn,
            this.filePathDataGridViewTextBoxColumn,
            this.isWhiteListedDataGridViewCheckBoxColumn,
            this.lastAnalysisDateDataGridViewTextBoxColumn,
            this.verdictDataGridViewTextBoxColumn,
            this.isSkippedDataGridViewCheckBoxColumn,
            this.skipReasonDataGridViewTextBoxColumn,
            this.malwareProbabilityDataGridViewTextBoxColumn,
            this.firstAnalysisDateDataGridViewTextBoxColumn,
            this.returnCodeDataGridViewTextBoxColumn});
            this.dataGridView1.DataSource = this.bindingSource1;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(984, 484);
            this.dataGridView1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.progressBar1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 428);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(984, 56);
            this.panel1.TabIndex = 2;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(128, 17);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(833, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 35);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(103, 20);
            this.button2.TabIndex = 2;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // sHA1DataGridViewTextBoxColumn
            // 
            this.sHA1DataGridViewTextBoxColumn.DataPropertyName = "SHA1";
            this.sHA1DataGridViewTextBoxColumn.HeaderText = "SHA1";
            this.sHA1DataGridViewTextBoxColumn.Name = "sHA1DataGridViewTextBoxColumn";
            this.sHA1DataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // filePathDataGridViewTextBoxColumn
            // 
            this.filePathDataGridViewTextBoxColumn.DataPropertyName = "FilePath";
            this.filePathDataGridViewTextBoxColumn.HeaderText = "FilePath";
            this.filePathDataGridViewTextBoxColumn.Name = "filePathDataGridViewTextBoxColumn";
            this.filePathDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // isWhiteListedDataGridViewCheckBoxColumn
            // 
            this.isWhiteListedDataGridViewCheckBoxColumn.DataPropertyName = "IsWhiteListed";
            this.isWhiteListedDataGridViewCheckBoxColumn.HeaderText = "IsWhiteListed";
            this.isWhiteListedDataGridViewCheckBoxColumn.Name = "isWhiteListedDataGridViewCheckBoxColumn";
            this.isWhiteListedDataGridViewCheckBoxColumn.ReadOnly = true;
            // 
            // lastAnalysisDateDataGridViewTextBoxColumn
            // 
            this.lastAnalysisDateDataGridViewTextBoxColumn.DataPropertyName = "LastAnalysisDate";
            this.lastAnalysisDateDataGridViewTextBoxColumn.HeaderText = "LastAnalysisDate";
            this.lastAnalysisDateDataGridViewTextBoxColumn.Name = "lastAnalysisDateDataGridViewTextBoxColumn";
            this.lastAnalysisDateDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // verdictDataGridViewTextBoxColumn
            // 
            this.verdictDataGridViewTextBoxColumn.DataPropertyName = "Verdict";
            this.verdictDataGridViewTextBoxColumn.HeaderText = "Verdict";
            this.verdictDataGridViewTextBoxColumn.Name = "verdictDataGridViewTextBoxColumn";
            this.verdictDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // isSkippedDataGridViewCheckBoxColumn
            // 
            this.isSkippedDataGridViewCheckBoxColumn.DataPropertyName = "IsSkipped";
            this.isSkippedDataGridViewCheckBoxColumn.HeaderText = "IsSkipped";
            this.isSkippedDataGridViewCheckBoxColumn.Name = "isSkippedDataGridViewCheckBoxColumn";
            this.isSkippedDataGridViewCheckBoxColumn.ReadOnly = true;
            // 
            // skipReasonDataGridViewTextBoxColumn
            // 
            this.skipReasonDataGridViewTextBoxColumn.DataPropertyName = "SkipReason";
            this.skipReasonDataGridViewTextBoxColumn.HeaderText = "SkipReason";
            this.skipReasonDataGridViewTextBoxColumn.Name = "skipReasonDataGridViewTextBoxColumn";
            this.skipReasonDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // malwareProbabilityDataGridViewTextBoxColumn
            // 
            this.malwareProbabilityDataGridViewTextBoxColumn.DataPropertyName = "MalwareProbability";
            this.malwareProbabilityDataGridViewTextBoxColumn.HeaderText = "MalwareProbability";
            this.malwareProbabilityDataGridViewTextBoxColumn.Name = "malwareProbabilityDataGridViewTextBoxColumn";
            // 
            // firstAnalysisDateDataGridViewTextBoxColumn
            // 
            this.firstAnalysisDateDataGridViewTextBoxColumn.DataPropertyName = "FirstAnalysisDate";
            this.firstAnalysisDateDataGridViewTextBoxColumn.HeaderText = "FirstAnalysisDate";
            this.firstAnalysisDateDataGridViewTextBoxColumn.Name = "firstAnalysisDateDataGridViewTextBoxColumn";
            // 
            // returnCodeDataGridViewTextBoxColumn
            // 
            this.returnCodeDataGridViewTextBoxColumn.DataPropertyName = "ReturnCode";
            this.returnCodeDataGridViewTextBoxColumn.HeaderText = "ReturnCode";
            this.returnCodeDataGridViewTextBoxColumn.Name = "returnCodeDataGridViewTextBoxColumn";
            // 
            // bindingSource1
            // 
            this.bindingSource1.DataSource = typeof(APTAT.Valkyrie.Client.AnalysisResult);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 484);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.dataGridView1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn sHA1DataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn filePathDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isWhiteListedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastAnalysisDateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn verdictDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isSkippedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn skipReasonDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn malwareProbabilityDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn firstAnalysisDateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn returnCodeDataGridViewTextBoxColumn;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button2;
    }
}