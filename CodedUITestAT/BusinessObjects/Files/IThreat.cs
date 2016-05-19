namespace CodedUITestAT
{
    /// <summary> Interface for a threat. </summary>
    internal interface IThreat
    {
        string Name { get; }
        string Location { get; }
        string SearchWord { get; }
    }
}
