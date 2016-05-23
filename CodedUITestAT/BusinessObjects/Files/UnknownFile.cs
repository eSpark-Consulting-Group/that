using System.IO;

namespace CodedUITestAT
{
    /// <summary> Represents an unknown file. </summary>
    internal sealed class UnknownFile : Threat
    {
        internal override string SearchWord { get { return _searchWord;} }

        /// <summary> Represents an unknown file. </summary>
        /// <param name="fileLocation">Set a local file location.</param>
        internal UnknownFile(string fileLocation) : base(fileLocation)
        {
            _searchWord = Settings.ScanCompleted.Default.UnknownFile;
        }

        private string _searchWord;
    }
}
