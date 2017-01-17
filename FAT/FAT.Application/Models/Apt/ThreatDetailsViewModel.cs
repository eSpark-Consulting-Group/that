
using System;
using System.IO;
using APTAT.Application.Models.Valkyrie;
using APTAT.Application.Services;
using APTAT.Core;
using APTAT.Core.Descent.Apt;

namespace APTAT.Application.Models
{
    public class ThreatDetailsViewModel : ViewModel
    {
        private IStorageService _storage;

        private String _fileName;
        private String _verdict;
        private String _size;
        private DateTime _created;
        private DateTime _modified;
        private DateTime _accessed;
        private String _hash;
        private String _signer;
        private String _description;
        private String _version;
        private String _copyright;
        private String _product;
        private Boolean _isMalware;
        private Boolean _isUnknown;
        private string _isWhitelisted;
        private string _malwareProbability;

        public String FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                RaisePropertyChanged("FileName");
            }
        }

        public String Verdict
        {
            get { return _verdict; }
            set
            {
                _verdict = value;
                RaisePropertyChanged("Verdict");
                RaisePropertyChanged("Clean");
                RaisePropertyChanged("Failed");
                RaisePropertyChanged("Pending");
            }
        }

        public String Size
        {
            get { return _size; }
            set
            {
                _size = value;
                RaisePropertyChanged("Size");
            }
        }

        public DateTime Created
        {
            get { return _created; }
            set 
            { 
                _created = value;
                RaisePropertyChanged("Created");
            }
        }

        public DateTime Modified
        {
            get { return _modified; }
            set
            {
                _modified = value;
                RaisePropertyChanged("Modified");
            }
        }

        public DateTime Accessed
        {
            get { return _accessed; }
            set
            {
                _accessed = value;
                RaisePropertyChanged("Accessed");
            }
        }

        public String Hash
        {
            get { return _hash; }
            set
            {
                _hash = value;
                RaisePropertyChanged("Hash");
            }
        }

        public String Signer
        {
            get { return _signer; }
            set
            {
                _signer = value;
                RaisePropertyChanged("Signer");
            }
        }

        public String Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChanged("Description");
            }
        }

        public String Version
        {
            get { return _version; }
            set
            {
                _version = value;
                RaisePropertyChanged("Version");
            }
        }

        public String Copyright
        {
            get { return _copyright; }
            set
            {
                _copyright = value;
                RaisePropertyChanged("Copyright");
            }
        }

        public String Product
        {
            get { return _product; }
            set
            {
                _product = value;
                RaisePropertyChanged("Product");
            }
        }

        public Boolean IsUnknown
        {
            get { return _isUnknown; }
            set
            {
                _isUnknown = value;
                RaisePropertyChanged("IsUnknown");
                RaisePropertyChanged("Clean");
            }
        }

        public bool Failed
        {
            get;
            set;
        }

        public bool Clean
        {
            get;
            set;
        }

        public bool Pending
        { 
            get; 
            set; 
        }

        public Boolean IsMalware
        {
            get { return _isMalware; }
            set
            {
                _isMalware = value;
                RaisePropertyChanged("IsMalware");
            }
        }

        public string IsWhitelisted
        {
            get { return _isWhitelisted; }
            set
            {
                _isWhitelisted = value;
                RaisePropertyChanged("IsWhitelisted");
            }
        }

        public string MalwareProbability
        {
            get { return _malwareProbability; }
            set
            {
                _malwareProbability = value;
                RaisePropertyChanged("MalwareProbability");
            }
        }

        public ThreatDetailsViewModel(IStorageService storage)
        {
            _storage = storage;
        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            var file = _storage.GetValue<Tuple<BadFileItem, ValkyrieFile>>(KnownKeys.UnknownFile, false);
            var badFile = file.Item1;
            var valkyrieFile = file.Item2;

            if (badFile == null && valkyrieFile == null)
                return;

            if (badFile != null)
            {
                switch (badFile.Cloud)
                {
                    case CloudVerdict.NoVerdict:
                    case CloudVerdict.FailedToAnalyze:
                        Failed = true;
                        Verdict = "Lookup in Progress";
                        break;
                    case CloudVerdict.FirstSeen:
                    case CloudVerdict.Unknown:
                        Verdict = "Unknown";
                        IsUnknown = true;
                        break;
                    case CloudVerdict.Malware:
                        Verdict = "Malware";
                        IsMalware = true;
                        break;
                    case CloudVerdict.Safe:
                        Verdict = "Clean";
                        Clean = true;
                        break;
                }
            }
            else
            {
                IsMalware = valkyrieFile.IsMalware;
                IsUnknown = valkyrieFile.IsUnknown;
                Clean = valkyrieFile.Clean;
                Failed = valkyrieFile.Failed;
                Pending = valkyrieFile.Pending;
                Verdict = valkyrieFile.VerdictString;
            }
            FileName = badFile != null ? Path.GetFileName(badFile.Name) : valkyrieFile.Name;
            Size = badFile != null ? badFile.FileSizeFormatted : valkyrieFile.FileSizeFormatted;
            Created = badFile != null ? badFile.Created.ToLocalTime() : valkyrieFile.Created.ToLocalTime();
            Modified = badFile != null ? badFile.Modified.ToLocalTime() : valkyrieFile.Modified.ToLocalTime();
            Accessed = badFile != null ? badFile.Accessed.ToLocalTime() : valkyrieFile.Accessed.ToLocalTime();
            Hash = NAIfEmpty(badFile != null ? badFile.Sha1?.ToUpper() : valkyrieFile.Sha1?.ToUpper());
            Signer = NAIfEmpty(badFile != null ? badFile.Signer : valkyrieFile.Signer);
            Description = NAIfEmpty(badFile != null ? badFile.Description : valkyrieFile.Description);
            Version = NAIfEmpty(badFile != null ? badFile.Version : valkyrieFile.Version);
            Copyright = NAIfEmpty(badFile != null ? badFile.Copyright : valkyrieFile.Copyright);
            Product = NAIfEmpty(badFile != null ? badFile.Product : valkyrieFile.Product);
            IsWhitelisted = valkyrieFile != null && valkyrieFile.IsWhiteListed ? "yes" : "no";
            MalwareProbability = (valkyrieFile != null && valkyrieFile.MalwareProbabilityValue.HasValue) ? 
                (valkyrieFile.MalwareProbabilityValue.Value / 100).ToString("P") : "";
        }

        private String NAIfEmpty(String str)
        {
            if(String.IsNullOrWhiteSpace(str))
            {
                return "n/a";
            }

            return str;
        }

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);
        }
    }
}
