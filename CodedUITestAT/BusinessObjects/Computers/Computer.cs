using System;

namespace CodedUITestAT
{
    /// <summary> Represents a computer. </summary>
    internal sealed class Computer : IRow
    {
        /// <summary> A network computer name. </summary>
        internal string Name { get { return _name; } }
        /// <summary> A computer row name at the application UI. </summary>
        public string RowName { get { return _rowName; } }

        /// <summary> Represents a local network computer. </summary>
        /// <param name="computerName">Set a computer isolated network name.</param>
        internal Computer(string computerName)
        {
            _name = computerName.ToLower();
            _rowName = _name;
        }

        /// <summary> Represents a current computer. </summary>
        internal Computer()
        {
            _name = Environment.MachineName.ToLower();
            _rowName = _name;
        }

        private string _name;
        private string _rowName;
    }
}
