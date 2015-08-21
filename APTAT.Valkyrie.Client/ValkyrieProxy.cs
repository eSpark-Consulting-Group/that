using APTAT.Valkyrie.Client.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;

namespace APTAT.Valkyrie.Client
{

    public sealed class ValkyrieProxy
    {

        public ConcurrentBag<AnalysisResult> AnalysisResults
        {
            get
            {
                return _analysisResults;
            }

            set
            {
                _analysisResults = value;
            }
        }

        public int TimeOut
        {
            get
            {
                return _timeout;
            }

            set
            {
                _timeout = value;
            }
        }

        ConcurrentBag<String> pendingRequests = new ConcurrentBag<String>();

        private ConcurrentBag<AnalysisResult> _analysisResults;

        public ValkyrieProxy(string clientID, int timeout = 300000)
        {
            _clientID = clientID;
            _timeout = timeout;

            AnalysisResults = new ConcurrentBag<AnalysisResult>();
            pendingRequests = new ConcurrentBag<string>();
        }
        const Int32 maxFileLenght = 26214400;
        private readonly string _clientID;
        private int _timeout;
        private string current = String.Empty;
        public AnalysisResult AskValkyrie(string filePath, string sha1)
        {
            try
            {

                if (pendingRequests.Contains(sha1))
                {
                    var time = _timeout;
                    while (_analysisResults.Any(a => a.SHA1 == sha1) == false && time > 0)
                    {
                        Thread.Sleep(1000);                    
                        time -= 1000;
                        Console.Write(".");
                    }
                    Console.WriteLine("");
                    var result2 = _analysisResults.Where(r => r.SHA1 == sha1).FirstOrDefault();
                    if (result2 != null)
                    {
                        return result2;
                    }
                    else
                    {
                        return AnalysisResult.CreateSkipInfo(sha1, "Timeout...... ");
                    }
                }
                else
                {
                    var foundResult = _analysisResults.Where(r => r.SHA1 == sha1).FirstOrDefault();
                    if (foundResult != null)
                    {                        
                        return foundResult;
                    }
                }

                pendingRequests.Add(sha1);
                var result = UploadFile(filePath, sha1);                
                return result;


            }
            catch (Exception e)
            {
                AddResult(AnalysisResult.CreateSkipInfo(filePath, e.Message));
            }
            finally
            {

            }

            return AnalysisResult.CreateSkipInfo(String.Empty, "Error");
        }

        private AnalysisResult UploadFile(string filePath, string sha1)
        {            
            FileInfo f = new FileInfo(filePath);
            if (f.Length > maxFileLenght)
            {
                var result = AnalysisResult.CreateSkipInfo(sha1, "Maximum file size per upload to 25 MB.");
                AddResult(result);
                return result;
            }

            var infoRequeast = new InfoRequest() { Client_id = _clientID, SHA1 = sha1 };
            BasicInfoResponse basicInfo = ValkyrieService.GetBasicInfo(infoRequeast);

            if (basicInfo.Presence)
            {
                AnalysisResult result = AnalysisResult.Create(basicInfo);
                AddResult(result);
                return result;
            }
            else
            {
                var request = new SubmitToAutomaticRequest();
                request.Client_id = _clientID;
                request.SubmitToken = basicInfo.SubmitToken;
                request.FilePath = filePath;


                var payload = request.ToNameValueCollection();

                var uploadResult = MultipartPostHelper.Post(request.FilePath, ValkyrieApiUrls.SubmitToAutomaticAnalysis.AbsoluteUri, payload);
                var response = JsonConvert.DeserializeObject<SubmitToAutomaticResponse>(uploadResult);
                var result = AnalysisResult.Create(response);
                AddResult(result);
                return result;

            }

        }

        private void AddResult(AnalysisResult result)
        {
            AnalysisResults.Add(result);
        }
    }
}
