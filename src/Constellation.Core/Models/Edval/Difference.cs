namespace Constellation.Core.Models.Edval;

using Enums;
using Identifiers;

public sealed class Difference
{
    public Difference(
        EdvalDifferenceType type,
        EdvalDifferenceSystem system,
        string identifier,
        string description,
        bool ignored = false)
    {
        Id = new();
        Type = type;
        System = system;
        Identifier = identifier;
        Description = description;
        Ignored = ignored;
    }

    private Difference() { }

    public DifferenceId Id { get; init; }
    public EdvalDifferenceType Type { get; init; }
    public EdvalDifferenceSystem System { get; init; }
    public string Identifier { get; init; }
    public string Description { get; init; }
    public bool Ignored { get; private set; }

    public void SetIgnored(bool value) => Ignored = value;
    public EdvalIgnore CreateIgnoreRecord() => new(Type, System, Identifier);
}