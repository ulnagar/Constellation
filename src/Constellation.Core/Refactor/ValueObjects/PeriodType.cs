namespace Constellation.Core.Refactor.ValueObjects;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

public class PeriodType : ValueObject
{
    static PeriodType() { }

    private PeriodType() { }

    private PeriodType(string code)
    {
        Code = code;
    }

    public static PeriodType From(string code)
    {
        var periodType = new PeriodType { Code = code };

        if (!SupportedPeriodTypes.Contains(periodType))
        {
            throw new UnsupportedPeriodTypeException(code);
        }

        return periodType;
    }

    public static PeriodType Teaching => new("Teaching");
    public static PeriodType Break => new("Break");
    public static PeriodType Offline => new("Offline");
    public static PeriodType Invisible => new("Invisible");
    public static PeriodType Unknown => new("Unknown");
    public string Code { get; private set; } = "Unknown";

    public static implicit operator string(PeriodType periodType)
    {
        return periodType.ToString();
    }

    public static explicit operator PeriodType(string code)
    {
        return From(code);
    }

    public override string ToString()
    {
        return Code;
    }

    protected static IEnumerable<PeriodType> SupportedPeriodTypes
    {
        get
        {
            yield return Teaching;
            yield return Break;
            yield return Offline;
            yield return Invisible;
            yield return Unknown;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

}
