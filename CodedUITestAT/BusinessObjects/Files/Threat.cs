using System;
using System.IO;

namespace CodedUITestAT
{
    /// <summary> Represents a general threat. </summary>
    internal abstract class Threat
    {
        /// <summary> A threat name. </summary>
        internal string Name { get { return _name; } }
        /// <summary> A threat location. </summary>
        internal string Location { get { return _location; } }
        /// <summary> A threat row name at application UI. </summary>
        internal string RowName { get { return _rowName; } }

        internal abstract string SearchWord { get; }

        /// <summary> Represents a general threat. </summary>
        /// <param name="fileLocation">Set a threat file location.</param>
        internal Threat(string fileLocation)
        {
            if (String.IsNullOrEmpty(fileLocation)) throw new Exception("The provided file location is either null or empty!");
            else if (!Path.IsPathRooted(fileLocation)) throw new Exception("The provided file location is not rooted!");
            else _location = fileLocation.ToLower();
            
            _name = Path.GetFileName(fileLocation).ToLower();

            _rowName = _location;
        }

        private string _name;
        private string _location;
        private string _rowName;
    }
}
