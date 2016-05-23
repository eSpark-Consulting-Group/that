using System;
using System.IO;

namespace CodedUITestAT
{
    /// <summary> Represents a general threat. </summary>
    internal abstract class Threat
    {
        internal string Name { get { return _name; } }
        internal string Location { get { return _location; } }
        internal string RowName { get { return _rowName; } }

        internal abstract string SearchWord { get; }

        /// <summary> Represents a general threat. </summary>
        /// <param name="fileLocation">Set a threat file location</param>
        internal Threat(string fileLocation)
        {
            if (String.IsNullOrEmpty(fileLocation)) throw new Exception("The provided file location is either null or empty!");
            else if (!Path.IsPathRooted(fileLocation)) throw new Exception("The provided file location is not rooted!");
            else _location = fileLocation;
            
            _name = Path.GetFileName(fileLocation);

            _rowName = _location;
        }

        private string _name;
        private string _location;
        private string _rowName;
    }
}
