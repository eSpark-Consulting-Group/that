namespace CodedUITestAT
{
    /// <summary> Represents a computer. </summary>
    internal sealed class Computer
    {
        internal string Name { get { return _name; } }
        internal string RowName { get { return _rowName; } }

        /// <summary> Represents a computer. </summary>
        /// <param name="computerName">Set a computer isolated network name.</param>
        internal Computer(string computerName)
        {
            _name = computerName;
            _rowName = computerName;
        }

        private string _name;
        private string _rowName;
    }
}
