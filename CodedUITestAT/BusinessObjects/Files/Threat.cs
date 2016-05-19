using System;
using System.IO;

namespace CodedUITestAT
{
    internal abstract class Threat
    {
        public string Name { get { return _name; } }
        public string Location { get { return _location; } }
        public abstract string SearchWord { get; }

        internal Threat(string fileLocation)
        {
            if (String.IsNullOrEmpty(fileLocation)) throw new Exception("The provided file location is either null or empty!");
            else _location = fileLocation;
            
            _name = Path.GetFileName(fileLocation);
        }

        private string _name;
        private string _location;
    }
}
