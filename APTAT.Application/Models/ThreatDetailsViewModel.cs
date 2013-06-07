
using System;
using System.IO;
using APTAT.Application.Services;

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

        public ThreatDetailsViewModel()
        {
            _storage = new StorageService();
        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            UnknownFileItem unknownFile = _storage.GetValue<UnknownFileItem>(KnownKeys.UnknownFile, false);



            FileName = Path.GetFileName(unknownFile.Name);
            Verdict = "unknown";
            Size = unknownFile.Size / 1024 + "KB";
            Created = DateTime.Now;
            Modified = DateTime.Now;
            Accessed = DateTime.Now;
            Hash = Guid.NewGuid().ToString();
            Signer = "Microsoft";
            Description = "Windows Explorer";
            Version = "6.2.9200.16433";
            Copyright = "© Microsoft Corporation";
            Product = "Microsoft Windows Operating Systems";
        }

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);
        }
    }
}
