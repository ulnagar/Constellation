namespace Constellation.Core.Models.Canvas.Models;

using Offerings;

public readonly record struct CanvasCourseCode
{
    public static CanvasCourseCode Empty => new(string.Empty);

    private readonly string _value;

    private CanvasCourseCode(string value) : this() => _value = value;
    
    public static CanvasCourseCode FromOffering(Offering offering)
    {
        string year = offering.EndDate.Year.ToString();

        return new($"{year}-{offering.Name.Value[..^2]}");
    }
    
    public static CanvasCourseCode FromValue(string value) =>
        new(value);

    public override string ToString() => _value;
}