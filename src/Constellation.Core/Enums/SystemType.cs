namespace Constellation.Core.Enums;

using Constellation.Core.Common;
using System.Collections.Generic;

public sealed class SystemType : StringEnumeration<SystemType>
{
    public static readonly SystemType Sentral = new("Sentral");

    private SystemType(string value)
        : base(value, value) { }

    public static IEnumerable<SystemType> GetOptions => GetEnumerable;
}