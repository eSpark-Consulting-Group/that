using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using APTAT.Valkyrie.Client;
using Valkyrie.WebApiProvider.Contracts;

namespace APTAT.Valkyrie.ClientTest
{
    public partial class CsvReaderForm : Form
    {
        public CsvReaderForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<BasicInfoResponse> results = new List<BasicInfoResponse>();
            var s = new StreamWriter("c:\\valkyrie.txt", true);

            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var hashCodes = CsvFileReader.ReadFile(dlg.FileName);

                string apikey = "b1511c93-6397-44fc-848f-65a4c9766294";

                var guid = Guid.NewGuid();
                foreach (var hash in hashCodes)
                {
                    BasicInfoResponse result = ValkyrieWebApiProvider.GetBasicInfo(new InfoRequest()
                    {
                        ApiKey = apikey,
                        EndpointId = guid,
                        SHA1 = hash
                    });

                    results.Add(result);
                    textBox1.Text += result.Verdict.ToString();
//                    this.basicInfoResponseBindingSource.DataSource = results;
                    s.WriteLine(result.SHA1 + " " + result.Presence + " " + (result.IsWhiteListed ? "Whitelisted" : "is not wl") + " " + result.MalwareProbability + " " + result.Verdict);
                }
                
                
                this.basicInfoResponseBindingSource.DataSource = results;
                s.Dispose();
            }
        }
    }
}
