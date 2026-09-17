namespace Constellation.Application.Extensions;

using Core.Enums;
using Core.Models.Offerings.ValueObjects;

public static class OfferingNameExtensions
{
    public static Grade? GetGrade(this OfferingName name)
    {
        string stringGrade = name.Value[..2];

        Grade? success = Grade.FromValue($"Y{stringGrade}");

        return success;
    }
}