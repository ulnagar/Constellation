namespace Constellation.Core.Extensions;

using Constellation.Core.Enums;

public static class GradeExtensions
{
    public static string AsNumber(this Grade grade) => ((int)grade).ToString();

    public static string AsName(this Grade grade) => $"Year {((int)grade).ToString().PadLeft(2, '0')}";
}