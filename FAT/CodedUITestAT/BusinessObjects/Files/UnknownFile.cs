namespace CodedUITestAT
{
    /// <summary> Represents an unknown file. </summary>
    internal sealed class UnknownFile : Threat
    {
        /// <summary> Search word to find the respective data in Scan Statistics. </summary>
        internal override string SearchWord { get { return _searchWord;} }
        /// <summary> A threat FLS verdict. </summary>
        internal override Verdict FileVerdict { get { return _fileVerdict; } }
        /// <summary> A threat Valkyrie verdict. </summary>
        internal override ValkyrieVerdict FileValkyrieVerdict { get { return _fileValkyrieVerdict; } }

        /// <summary> Represents an unknown file. </summary>
        /// <param name="fileLocation">Set a local file location.</param>
        /// <param name="machine">Set a machine, containing the threat. Leave null for a computer running the test.</param>
        internal UnknownFile(string fileLocation, Computer machine = null)
            : base(fileLocation, machine)
        {
            _searchWord = Settings.ScanCompleted.Default.UnknownFile;
            _fileVerdict = Verdict.Unknown;
            _fileValkyrieVerdict = ValkyrieVerdict.InAnalysis;
        }
        
        private string _searchWord;
        private Verdict _fileVerdict;
        private ValkyrieVerdict _fileValkyrieVerdict;
    }
}
