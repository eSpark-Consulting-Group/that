
using System;
using System.IO;
using FAT.Application.Services;
using APTAT.Core;
using APTAT.Core.Descent.Apt;

namespace FAT.Application.Models
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

            var badFile = _storage.GetValue<ThreatInfoItem>(KnownKeys.UnknownFile, false);

            if (badFile == null)
                return;
            
            IsMalware = badFile.IsMalware;
            IsUnknown = badFile.IsUnknown;
            Clean = badFile.Clean;
            Failed = badFile.Failed;
            Pending = badFile.Pending;
                       
            Verdict = badFile.VerdictString;

            FileName = Path.GetFileName(badFile.Name);
            Size = badFile.BadFile.FileSizeFormatted;
            Created = badFile.BadFile.Created.ToLocalTime();
            Modified = badFile.BadFile.Modified.ToLocalTime();
            Accessed = badFile.BadFile.Accessed.ToLocalTime();
            Hash = NAIfEmpty(badFile.BadFile.Sha1?.ToUpper());
            Signer = NAIfEmpty(badFile.BadFile.Signer);
            Description = NAIfEmpty(badFile.BadFile.Description);
            Version = NAIfEmpty(badFile.BadFile.Version);
            Copyright = NAIfEmpty(badFile.BadFile.Copyright);
            Product = NAIfEmpty(badFile.BadFile.Product);
            IsWhitelisted = badFile.IsWhiteListed ? "yes" : "no";
            MalwareProbability = badFile.MalwareProbabilityValue.HasValue ? 
                (badFile.MalwareProbabilityValue.Value / 100).ToString("P") : "";
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
