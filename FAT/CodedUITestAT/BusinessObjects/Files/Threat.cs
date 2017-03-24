using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CodedUITestFAT
{
    /// <summary> Represents a general threat. </summary>
    internal abstract class Threat : IRow
    {
        /// <summary> A threat name. </summary>
        internal string Name { get { return _name; } }
        /// <summary> A threat location. </summary>
        internal string Location { get { return _location; } }
        /// <summary> A threat row name at application UI. </summary>
        public string RowName { get { return _rowName; } }
        /// <summary> A threat size in kbytes. </summary>
        internal long Size { get { return _size; } }
        /// <summary> A threat size in kbytes. </summary>
        internal string DisplaySize { get { return _displaySize; } }
        /// <summary> Date and time when a threat was created. </summary>
        internal string Created { get { return _created; } }
        /// <summary> Date and time when a threat was modified. </summary>
        internal string Modified { get { return _modified; } }
        /// <summary> Date and time when a threat was accessed. </summary>
        internal string Accessed { get { return _accessed; } }
        /// <summary> A threat SHA1 hash. </summary>
        internal string Sha1 { get { return _sha1; } }
        /// <summary> A threat signer. </summary>
        internal string Signer { get { return _signer; } }
        /// <summary> A threat description. </summary>
        internal string Description { get { return _description; } }
        /// <summary> A threat version. </summary>
        internal string Version { get { return _version; } }
        /// <summary> A threat copyright. </summary>
        internal string Copyright { get { return _copyright; } }
        /// <summary> A threat product name. </summary>
        internal string Product { get { return _product; } }
        /// <summary> A threat trusted vendor name. </summary>
        internal string TrustedVendor { get { return _trustedVendor; } }
        /*
        /// <summary> A threat malware probability. </summary>
        internal string MalwareProbability { get { return _malwareProbability; } }
        */
        /// <summary> Search word to find the respective data in Scan Statistics. </summary>
        internal abstract string SearchWord { get; }
        /// <summary> A threat verdict. </summary>
        internal abstract Verdict FileVerdict { get; }
        /// <summary> A threat Valkyrie verdict. </summary>
        internal abstract ValkyrieVerdict FileValkyrieVerdict { get; }

        /// <summary> Represents a general threat. </summary>
        /// <param name="fileLocation">Set a threat location.</param>
        /// <param name="machine">Set a machine, containing the threat. Set null for a computer running the test.</param>
        internal Threat(string fileLocation, Computer machine)
        {
            string networkPath;
            var culture = CultureInfo.GetCultureInfo("en-Us");

            if (String.IsNullOrEmpty(fileLocation)) throw new Exception("The provided file location is either null or empty!");
            else if (!Path.IsPathRooted(fileLocation)) throw new Exception("The provided file location is not rooted!");
            else _location = fileLocation.ToLower();

            // Formates the path to the threat
            if (machine == null) networkPath = fileLocation;
            else
            { 
              string root = Path.GetPathRoot(fileLocation);
              string noRootLocation = fileLocation.Replace(root, String.Empty);
              networkPath = String.Format(@"\\{0}\{1}\{2}", machine.Name, DataHelper.GetEndpointRoot(true), noRootLocation);
            }            
            
            _name = Path.GetFileName(fileLocation);

            _rowName = _location;

            FileInfo fileInfo = new FileInfo(networkPath);

            _size = (long) (fileInfo.Length / 1024);

            _displaySize = _size.ToString() + " KB";

            _created = File.GetCreationTime(networkPath).ToString(culture);

            _modified = File.GetLastWriteTime(networkPath).ToString(culture);

            _accessed = File.GetLastAccessTime(networkPath).ToString(culture);

            _sha1 = SysHelper.GetFileSha1(networkPath);

            try
            {                                
                var certificate = X509Certificate.CreateFromSignedFile(networkPath);
                string issuer = certificate.Issuer;
                string searchWord = @"O=";
                string searchPattern = String.Format(@"{0}.*?(?=,)", searchWord);
                _signer = Regex.Match(issuer, searchPattern).ToString().Substring(searchWord.Length);
            }
            catch (CryptographicException) { _signer = Settings.General.Default.NoValueAvailable; }

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(networkPath);

            _description = fileVersionInfo.FileDescription;

            _version = fileVersionInfo.FileVersion;

            _copyright = fileVersionInfo.LegalCopyright;

            _product = fileVersionInfo.ProductName;

            _trustedVendor = Settings.General.Default.ThreatTrustedVendor;

            //_malwareProbability = Settings.General.Default.NoValueAvailable;
        }

        private string _name;
        private string _location;
        private string _rowName;
        private long _size;
        private string _displaySize;
        private string _created;
        private string _modified;
        private string _accessed;
        private string _sha1;
        private string _signer;
        private string _description;
        private string _version;
        private string _copyright;
        private string _product;
        private string _trustedVendor;
        //private string _malwareProbability;
    }
}
