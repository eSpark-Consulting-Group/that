using System.IO;

namespace CodedUITestAT
{
    /// <summary> Represents an unknown file. </summary>
    internal sealed class UnknownFile : Threat
    {
        /// <summary> Search word to find the respective data in Scan Statistics. </summary>
        internal override string SearchWord { get { return _searchWord;} }
        /// <summary> A threat verdict. </summary>
        internal override Verdict FileVerdict { get { return _fileVerdict; } }

        /// <summary> Represents an unknown file. </summary>
        /// <param name="fileLocation">Set a local file location.</param>
        /// <param name="machine">Set a machine, containing the threat. Leave null for a computer running the test.</param>
        internal UnknownFile(string fileLocation, Computer machine = null)
            : base(fileLocation, machine)
        {
            _searchWord = Settings.ScanCompleted.Default.UnknownFile;
            _fileVerdict = Verdict.Unknown;
        }

        private string _searchWord;
        private Verdict _fileVerdict;
    }
}
