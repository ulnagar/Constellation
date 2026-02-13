namespace Constellation.Application.Extensions;

using Core.Enums;
using Core.Models.Offerings.ValueObjects;

public static class OfferingNameExtensions
{
    public static Grade? GetGrade(this OfferingName name)
    {
        string stringGrade = name.Value[..2];

        bool success = Enum.TryParse($"Y{stringGrade}", true, out Grade grade);

        if (!success)
            return null;

        return grade;
    }
}