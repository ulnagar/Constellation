namespace Constellation.Core.Refactor.ValueObjects;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

public class PositionType : ValueObject
{
    static PositionType() { }

    private PositionType() { }

    private PositionType(string code)
    {
        Code = code;
    }

    public static PositionType From(string code)
    {
        var positionType = new PositionType { Code = code };

        if (!SupportedPositionTypes.Contains(positionType))
        {
            throw new UnsupportedPositionTypeException(code);
        }

        return positionType;
    }

    public static PositionType ACC => new("Aurora College Coordinator");
    public static PositionType SPT => new("Science Practical Teacher");
    public static PositionType Principal => new("Principal");
    public static PositionType Unknown => new("Unknown");
    public string Code { get; private set; } = "Unknown";

    public static implicit operator string(PositionType positionType)
    {
        return positionType.ToString();
    }

    public static explicit operator PositionType(string code)
    {
        return From(code);
    }

    public override string ToString()
    {
        return Code;
    }

    protected static IEnumerable<PositionType> SupportedPositionTypes
    {
        get
        {
            yield return ACC;
            yield return SPT;
            yield return Principal;
            yield return Unknown;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

}
