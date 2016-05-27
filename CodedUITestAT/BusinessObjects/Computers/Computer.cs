using System;

namespace CodedUITestAT
{
    /// <summary> Represents a computer. </summary>
    internal sealed class Computer
    {
        internal string Name { get { return _name; } }
        internal string RowName { get { return _rowName; } }

        /// <summary> Represents a local network computer. </summary>
        /// <param name="computerName">Set a computer isolated network name.</param>
        internal Computer(string computerName)
        {
            _name = computerName;
            _rowName = computerName;
        }

        /// <summary> Represents a current computer. </summary>
        internal Computer()
        {
            _name = Environment.MachineName;
            _rowName = _name;
        }

        private string _name;
        private string _rowName;
    }
}
