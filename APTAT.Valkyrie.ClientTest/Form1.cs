using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using APTAT.Valkyrie.Client;

namespace APTAT.Valkyrie.ClientTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _client = new Client.ValkyrieClient("MYCLIENTID");
            _client.AnalyzeCompleted += _client_AnalyzeCompleted;
            _client.AnalyzeProgressChanged += _client_AnalyzeProgressChanged;
            _client.AnalyzeCanceled += _client_AnalyzeCanceled;
        }

        void _client_AnalyzeCanceled(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void _client_AnalyzeProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            this.Text = String.Format("Scanned {0:P}", e.ProgressPercentage / 100m);
        }

        void _client_AnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<AnalysisResult> e)
        {
            bindingSource1.DataSource = e.Result;
            button1.Enabled = true;
        }

        private Valkyrie.Client.ValkyrieClient _client;
        private const string folderPath = @"C:\Users\Yevhen.Bal\AppData\Local\Temp\ValkyrieTmp";
        private static List<ValkyrieFileInfo> MockData()
        {
            var files = new List<ValkyrieFileInfo>();
            const string ip = "10.100.65.42";

            files.Add(new ValkyrieFileInfo() { Ip = ip, Path = "WMM2AE.dll", Sha1 = "" });
            files.Add(new ValkyrieFileInfo() { Ip = ip, Path = "WMM2ERES.dll", Sha1 = "" });
            files.Add(new ValkyrieFileInfo() { Ip = ip, Path = "WMM2EXT.dll", Sha1 = "" });
            files.Add(new ValkyrieFileInfo() { Ip = ip, Path = "WMM2FILT.dll", Sha1 = "" });
            files.Add(new ValkyrieFileInfo() { Ip = ip, Path = "WMM2FXA.dll", Sha1 = "" });
            files.Add(new ValkyrieFileInfo() { Ip = ip, Path = "WMM2FXB.dll", Sha1 = "" });
            files.Add(new ValkyrieFileInfo() { Ip = ip, Path = "WMM2RES.dll", Sha1 = "" });
            files.Add(new ValkyrieFileInfo() { Ip = ip, Path = "WMM2RES2.dll", Sha1 = "" });
            return files;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            bindingSource1.DataSource = null;
            button1.Enabled = false;


            var files = MockData();
            _client.AnalyzeAsync(files,"user", "test", 1);

        }

       
    }
}
