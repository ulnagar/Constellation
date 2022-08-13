namespace Constellation.Core.Refactor.ValueObjects;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

public class MSTeamType : ValueObject
{
    static MSTeamType() { }

    private MSTeamType() { }

    private MSTeamType(string code)
    {
        Code = code;
    }

    public static MSTeamType From(string code)
    {
        var msTeamType = new MSTeamType { Code = code };

        if (!SupportedMSTeamTypes.Contains(msTeamType))
        {
            throw new UnsupportedMSTeamTypeException(code);
        }

        return msTeamType;
    }

    public static MSTeamType Class => new("Class");
    public static MSTeamType WholeSchool => new("WholeSchool");
    public static MSTeamType Tutorial => new("Tutorial");
    public static MSTeamType Faculty => new("Faculty");
    public static MSTeamType Extracurricular => new("Extracurricular");
    public static MSTeamType Unknown => new("Unknown");
    public string Code { get; private set; } = "Unknown";

    public static implicit operator string(MSTeamType msTeamType)
    {
        return msTeamType.ToString();
    }

    public static explicit operator MSTeamType(string code)
    {
        return From(code);
    }

    public override string ToString()
    {
        return Code;
    }

    protected static IEnumerable<MSTeamType> SupportedMSTeamTypes
    {
        get
        {
            yield return Class;
            yield return WholeSchool;
            yield return Tutorial;
            yield return Faculty;
            yield return Extracurricular;
            yield return Unknown;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

}
