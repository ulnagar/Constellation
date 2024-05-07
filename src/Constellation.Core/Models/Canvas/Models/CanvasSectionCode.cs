namespace Constellation.Core.Models.Canvas.Models;

using Offerings;

public readonly record struct CanvasSectionCode
{
    public static CanvasSectionCode Empty => new(string.Empty);

    private readonly string _value;

    private CanvasSectionCode(string value) : this() => _value = value;
    
    public static CanvasSectionCode FromOffering(Offering offering)
    {
        string year = offering.EndDate.Year.ToString();

        return new($"{year}-{offering.Name.Value}");
    }

    public static CanvasSectionCode FromValue(string value) =>
        new(value);

    public override string ToString() => _value;
}