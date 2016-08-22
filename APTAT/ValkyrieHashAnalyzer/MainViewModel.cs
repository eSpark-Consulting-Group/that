using APTAT.Valkyrie.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Valkyrie.WebApiProvider.Contracts;

namespace ValkyrieHashAnalyzer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ValkyrieClient _client;
        private int _progress;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            var tmp = PropertyChanged;
            if (tmp != null)
            {
                tmp(this, new PropertyChangedEventArgs(name));
            }
        }

        public MainViewModel(ValkyrieClient client)
        {
            Progress = 0;
            Information = "APT ";
            _client = client;

            _client.AnalyzeProgressChanged += _client_AnalyzeProgressChanged;
            _client.OnResultAdded += _client_OnResultAdded;
            ValkyrieResults = new ObservableCollection<ValkyrieAnalysisResult>();
        }

        private void _client_OnResultAdded(object sender, ProgressChangedEventArgs e)
        {
            ValkyrieResults.Add((ValkyrieAnalysisResult)e.UserState);
            OnPropertyChanged("ValkyrieResults");
        }

        private ProgressInfo<ValkyrieAnalysisResult> _result;

        private void _client_AnalyzeProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {

            var p = (ProgressInfo<ValkyrieAnalysisResult>)e.UserState;
            Progress = e.ProgressPercentage;
            Information = "Valkyrie Hunter - " + "Processed " + p.ProcessedFileCount + " from " + p.TotalFileCount + " Malware found " + p.MalwareFilesCount;
        }

        internal void SaveToFile(string fileName)
        {
            using (var s = new StreamWriter(fileName, false))
            {
                var realResults = ValkyrieResults.Where(r => r.IsSkipped == false).ToList();
                foreach (var result in realResults)
                {
                    var p = result.MalwareProbability / 100;
                    var record = String.Format("{0},{1},{2},{3}",
                        result.SHA1,
                        (result.IsWhiteListed ? "Whitelisted" : ""),
                        result.VerdictString,
                        p.ToString("P", CultureInfo.InvariantCulture));

                    s.WriteLine(record);
                }
            }
        }

        ObservableCollection<ValkyrieAnalysisResult> _ValkyrieResults;
        public ObservableCollection<ValkyrieAnalysisResult> ValkyrieResults
        {
            get
            {

                return _ValkyrieResults;
            }
            set
            {
                _ValkyrieResults = value;
                OnPropertyChanged("ValkyrieResults");
            }
        }

        public ProgressInfo<ValkyrieAnalysisResult> CurrentResult
        {
            get
            {
                return _result;
            }

            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
        }

        string _information;
        public string Information
        {
            set
            {
                _information = value;
                OnPropertyChanged("Information");
            }
            get
            {
                return _information;
            }
        }

        public int Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public RegisterWithResponse Register(string camLicenseId, string camUserName, string camPassword, NetworkCredential proxyCredential)
        {
            var camRegistration = new CamRegistration(camLicenseId, camUserName, camPassword);
            RegisterWithResponse regInfo = _client.Register(camRegistration, proxyCredential.UserName, proxyCredential.Password);

            return regInfo;
        }



        public void Analyze(string fileName)
        {
            Progress = 0;
            
            var hashCodes = CsvFileReader.ReadFile(fileName).ToList();
         //   string apikey = "b1511c93-6397-44fc-848f-65a4c9766294";

            var regInfo = ValkyrieWebApiProvider.RegisterWithCamLicense(new RegisterWithCamLicenseRequest()
            {
                LicenseId = Properties.Settings.Default.CamLicense,
                ProductName = "Valkyrie Hunter",
                ProductVersion = "1.1"
            });

            if (regInfo.ReturnCode != 0)
            {
                throw new InvalidOperationException(regInfo.ResultDescription);
            }
            _client.AnalyzeAsync(hashCodes, regInfo.ApiKey);

        }


    }

}
