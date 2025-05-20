namespace Constellation.Core.Models.Edval;

using Enums;

public sealed class EdvalIgnore
{
    public EdvalIgnore(
        EdvalDifferenceType type,
        EdvalDifferenceSystem system,
        string identifier)
    {
        Type = type;
        System = system;
        Identifier = identifier;
    }

    private EdvalIgnore() { }

    public EdvalDifferenceType Type { get; init; }
    public EdvalDifferenceSystem System { get; init; }
    public string Identifier { get; init; }
}