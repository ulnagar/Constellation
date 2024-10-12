using Constellation.Core.Common;
using System.Collections.Generic;

namespace Constellation.Core.Models.Students.Enums;

public sealed class SystemType : StringEnumeration<SystemType>
{
    public static readonly SystemType Sentral = new("Sentral");

    private SystemType(string value)
        : base(value, value) { }

    public static IEnumerable<SystemType> GetOptions => GetEnumerable;
}