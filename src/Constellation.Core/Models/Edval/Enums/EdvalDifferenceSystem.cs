namespace Constellation.Core.Models.Edval.Enums;

using Common;

public sealed class EdvalDifferenceSystem : StringEnumeration<EdvalDifferenceSystem>
{
    public static readonly EdvalDifferenceSystem None = new(string.Empty);

    public static readonly EdvalDifferenceSystem EdvalDifference = new("Edval");
    public static readonly EdvalDifferenceSystem ConstellationDifference = new("Constellation");

    public EdvalDifferenceSystem(string value)
        : base(value, value)
    { }
}