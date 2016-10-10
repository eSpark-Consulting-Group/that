using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Valkyrie.WebApiProvider;
using Valkyrie.WebApiProvider.Contracts;

namespace Aptat.Valkyrie.ClientTest
{
    public partial class CsvReaderForm : Form
    {
        public CsvReaderForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var results = new List<BasicInfoResponse>();
            var s = new StreamWriter("c:\\valkyrie.txt", true);

            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var hashCodes = CsvFileReader.ReadFile(dlg.FileName);

                string apikey = "b1511c93-6397-44fc-848f-65a4c9766294";

                var guid = Guid.NewGuid();
                foreach (var hash in hashCodes)
                {
                    var arg = new InfoRequest(hash, guid, String.Empty, String.Empty, String.Empty);
                    BasicInfoResponse result = ValkyrieWebApiProvider.GetBasicInfo(arg);                         

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
