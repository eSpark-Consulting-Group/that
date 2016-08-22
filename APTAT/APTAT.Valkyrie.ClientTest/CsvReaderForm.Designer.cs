using Valkyrie.WebApiProvider.Contracts;

namespace Aptat.Valkyrie.ClientTest
{
    partial class CsvReaderForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.firstAnalysisDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastAnalysisDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.malwareProbabilityDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.presenceDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.returnCodeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sHA1DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.submitTokenDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.verdictDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isWhiteListedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.basicInfoResponseBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.basicInfoResponseBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 271);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1126, 320);
            this.panel1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(998, 10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 27);
            this.button1.TabIndex = 1;
            this.button1.Text = "Ask Valkyrie";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dataGridView1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1126, 271);
            this.panel2.TabIndex = 1;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.firstAnalysisDateDataGridViewTextBoxColumn,
            this.lastAnalysisDateDataGridViewTextBoxColumn,
            this.malwareProbabilityDataGridViewTextBoxColumn,
            this.presenceDataGridViewCheckBoxColumn,
            this.returnCodeDataGridViewTextBoxColumn,
            this.sHA1DataGridViewTextBoxColumn,
            this.submitTokenDataGridViewTextBoxColumn,
            this.verdictDataGridViewTextBoxColumn,
            this.isWhiteListedDataGridViewCheckBoxColumn});
            this.dataGridView1.DataSource = this.basicInfoResponseBindingSource;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(1126, 271);
            this.dataGridView1.TabIndex = 2;
            // 
            // firstAnalysisDateDataGridViewTextBoxColumn
            // 
            this.firstAnalysisDateDataGridViewTextBoxColumn.DataPropertyName = "FirstAnalysisDate";
            this.firstAnalysisDateDataGridViewTextBoxColumn.HeaderText = "FirstAnalysisDate";
            this.firstAnalysisDateDataGridViewTextBoxColumn.Name = "firstAnalysisDateDataGridViewTextBoxColumn";
            // 
            // lastAnalysisDateDataGridViewTextBoxColumn
            // 
            this.lastAnalysisDateDataGridViewTextBoxColumn.DataPropertyName = "LastAnalysisDate";
            this.lastAnalysisDateDataGridViewTextBoxColumn.HeaderText = "LastAnalysisDate";
            this.lastAnalysisDateDataGridViewTextBoxColumn.Name = "lastAnalysisDateDataGridViewTextBoxColumn";
            // 
            // malwareProbabilityDataGridViewTextBoxColumn
            // 
            this.malwareProbabilityDataGridViewTextBoxColumn.DataPropertyName = "MalwareProbability";
            this.malwareProbabilityDataGridViewTextBoxColumn.HeaderText = "MalwareProbability";
            this.malwareProbabilityDataGridViewTextBoxColumn.Name = "malwareProbabilityDataGridViewTextBoxColumn";
            // 
            // presenceDataGridViewCheckBoxColumn
            // 
            this.presenceDataGridViewCheckBoxColumn.DataPropertyName = "Presence";
            this.presenceDataGridViewCheckBoxColumn.HeaderText = "Presence";
            this.presenceDataGridViewCheckBoxColumn.Name = "presenceDataGridViewCheckBoxColumn";
            // 
            // returnCodeDataGridViewTextBoxColumn
            // 
            this.returnCodeDataGridViewTextBoxColumn.DataPropertyName = "ReturnCode";
            this.returnCodeDataGridViewTextBoxColumn.HeaderText = "ReturnCode";
            this.returnCodeDataGridViewTextBoxColumn.Name = "returnCodeDataGridViewTextBoxColumn";
            // 
            // sHA1DataGridViewTextBoxColumn
            // 
            this.sHA1DataGridViewTextBoxColumn.DataPropertyName = "SHA1";
            this.sHA1DataGridViewTextBoxColumn.HeaderText = "SHA1";
            this.sHA1DataGridViewTextBoxColumn.Name = "sHA1DataGridViewTextBoxColumn";
            // 
            // submitTokenDataGridViewTextBoxColumn
            // 
            this.submitTokenDataGridViewTextBoxColumn.DataPropertyName = "SubmitToken";
            this.submitTokenDataGridViewTextBoxColumn.HeaderText = "SubmitToken";
            this.submitTokenDataGridViewTextBoxColumn.Name = "submitTokenDataGridViewTextBoxColumn";
            // 
            // verdictDataGridViewTextBoxColumn
            // 
            this.verdictDataGridViewTextBoxColumn.DataPropertyName = "Verdict";
            this.verdictDataGridViewTextBoxColumn.HeaderText = "Verdict";
            this.verdictDataGridViewTextBoxColumn.Name = "verdictDataGridViewTextBoxColumn";
            // 
            // isWhiteListedDataGridViewCheckBoxColumn
            // 
            this.isWhiteListedDataGridViewCheckBoxColumn.DataPropertyName = "IsWhiteListed";
            this.isWhiteListedDataGridViewCheckBoxColumn.HeaderText = "IsWhiteListed";
            this.isWhiteListedDataGridViewCheckBoxColumn.Name = "isWhiteListedDataGridViewCheckBoxColumn";
            // 
            // basicInfoResponseBindingSource
            // 
            this.basicInfoResponseBindingSource.DataSource = typeof(BasicInfoResponse);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(59, 6);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(664, 265);
            this.textBox1.TabIndex = 2;
            this.textBox1.WordWrap = false;
            // 
            // CsvReaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1126, 591);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "CsvReaderForm";
            this.Text = "CsvReaderForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.basicInfoResponseBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridViewTextBoxColumn firstAnalysisDateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastAnalysisDateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn malwareProbabilityDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn presenceDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn returnCodeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn sHA1DataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn submitTokenDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn verdictDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isWhiteListedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.BindingSource basicInfoResponseBindingSource;
        private System.Windows.Forms.TextBox textBox1;
    }
}