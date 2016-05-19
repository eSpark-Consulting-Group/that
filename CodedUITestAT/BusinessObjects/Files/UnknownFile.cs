using System.IO;

namespace CodedUITestAT
{
    internal sealed class UnknownFile : IThreat
    {
        public string Name { get { return _name; } }
        public string Location { get { return _location; } }
        public string SearchWord {  get {return _searchWord;} }

        internal UnknownFile(string fileLocation)
        {
            _location = fileLocation;
            _name = Path.GetFileName(fileLocation);
            _searchWord = Settings.ScanCompleted.Default.UnknownFiles;        
        }

        private string _name;
        private string _location;
        private string _searchWord;
    }
}
