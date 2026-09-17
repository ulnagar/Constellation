namespace Constellation.Application.Extensions;

using Core.Enums;
using Core.Models.Tutorials.ValueObjects;

public static class TutorialNameExtensions
{
    public static Grade? GetGrade(this TutorialName name)
    {
        string stringGrade = name.Value[..2];

        Grade? success = Grade.FromValue($"Y{stringGrade}");

        return success;
    }
}