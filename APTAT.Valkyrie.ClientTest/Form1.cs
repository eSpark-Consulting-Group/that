using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using APTAT.Valkyrie.Client;
using APTAT.Valkyrie.Client.Contracts;

namespace APTAT.Valkyrie.ClientTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _client = new ValkyrieClient();
            _client.AnalyzeCompleted += _client_AnalyzeCompleted;
            _client.AnalyzeProgressChanged += _client_AnalyzeProgressChanged;

        }

        void _client_AnalyzeProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            this.Text = String.Format("Scanned {0:P}", e.ProgressPercentage / 100m);
        }

        void _client_AnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<AnalysisResult> e)
        {
            var isCancelled = e.Cancelled;
            if (isCancelled) MessageBox.Show("Cancelled");
            bindingSource1.DataSource = e.Result;
            button1.Enabled = true;
        }

        private ValkyrieClient _client;
      //  private const string folderPath = @"C:\Users\Yevhen.Bal\AppData\Local\Temp\ValkyrieTmp";

        private void button1_Click(object sender, EventArgs e)
        {

            bindingSource1.DataSource = null;


            var files = MockData.GetBadFiles();

            var reg = _client.Register(new CamRegistration("adsf", "adsf","adsf"));
            //   _client.AnalyzeAsync(files, "user", "test", 1, apiKey);
            // SubmitToManualFromBytes(files.First(), apiKey);
            ValkyrieFileInfo filesFirst = files.First();
            SubmitToManualTest(filesFirst, reg.ApiKey, filesFirst.Path);

        }

        private SubmitToManualResponse SubmitToManualFromBytes(ValkyrieFileInfo file, string apiKey)
        {
            var filePath = "C:\\Temp\\7DDB5C8B108B46D773F4A88954E02401CEE9E624.tmp";

            SubmitToManualResponse response = null;
            var bytes = File.ReadAllBytes(filePath);
            var info = ValkyrieService.GetBasicInfo(new InfoRequest() { ApiKey = apiKey, SHA1 = file.Sha1 });

            if (info.Presence)
            {
                var req1 = new SubmitToManualRequestSHA1()
                {
                    ApiKey = apiKey,
                    FilePath = filePath,
                    Priority = 3,
                    SHA1 = file.Sha1,
                    SubmitToken = info.SubmitToken
                };

                return ValkyrieService.SubmitToManualAnalysisWithoutUpload(req1);
            }
            else
            {
                var request = new SubmitToManualRequest()
                {
                    ApiKey = apiKey,
                    FilePath = filePath, //where file was found
                    Priority = 3,
                    SubmitToken = info.SubmitToken
                };

                response = ValkyrieService.SubmitToManualAnalysis(request, bytes, CancellationToken.None);
            }

            return response;
        }

        private SubmitToManualResponse SubmitToManualTest(ValkyrieFileInfo file, string apiKey, string filePath)
        {
        

            var info = ValkyrieService.GetBasicInfo(new InfoRequest() { ApiKey = apiKey, SHA1 = file.Sha1 });

            if (info.Presence)
            {
                return ValkyrieService.SubmitToManualAnalysisWithoutUpload(new SubmitToManualRequestSHA1()
                {
                    ApiKey = apiKey,
                    FilePath = filePath,
                    Priority = 3,
                    SHA1 = file.Sha1,
                    SubmitToken = info.SubmitToken
                });
            }
            else
            {
                return ValkyrieService.SubmitToManualAnalysis(new SubmitToManualRequest()
                {
                    ApiKey = apiKey,                    
                    FilePath = filePath,
                    Priority = 3,
                    SubmitToken = info.SubmitToken
                }, filePath, CancellationToken.None);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var reg = _client.Register(new CamRegistration("adsf", "adsf", "asdf"));            
            var path = "C:\\Temp\\C631FB17389FF624E7B606D1006342E032BE28D7.tmp";
            var result = _client.SubmitToManual(path, reg.ApiKey);
            var status = ValkyrieService.ManualStatus(new ManualStatusRequest(result.ManualAnalysisId, reg.ApiKey));
            
        }


    }
}
