namespace Constellation.Core.Extensions;

using Constellation.Core.Enums;
using System.Globalization;

public static class GradeExtensions
{
    extension(Grade grade)
    {
        public string AsNumber() => grade.Order.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
        public string AsName() => grade.Name;
    }
}