namespace CodedUITestAT
{
    internal interface IThreat
    {
        string Name { get; }
        FileType Type { get; }
        string Path { get; }
    }
}
