namespace Constellation.Core.Refactor.ValueObjects;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

public class TeacherType : ValueObject
{
    static TeacherType() { }

    private TeacherType() { }

    private TeacherType(string code)
    {
        Code = code;
    }

    public static TeacherType From(string code)
    {
        var teacherType = new TeacherType { Code = code };

        if (!SupportedTeacherTypes.Contains(teacherType))
        {
            throw new UnsupportedTeacherTypeException(code);
        }

        return teacherType;
    }

    public static TeacherType Primary => new("Primary");
    public static TeacherType Tutorial => new("Tutorial");
    public static TeacherType Supporting => new("Supporting");
    public static TeacherType Supervising => new("Supervising");
    public static TeacherType Unknown => new("Unknown");

    public string Code { get; private set; } = "Unknown";

    public static implicit operator string(TeacherType teacherType)
    {
        return teacherType.ToString();
    }

    public static explicit operator TeacherType(string code)
    {
        return From(code);
    }

    public override string ToString()
    {
        return Code;
    }

    protected static IEnumerable<TeacherType> SupportedTeacherTypes
    {
        get
        {
            yield return Primary;
            yield return Tutorial;
            yield return Supporting;
            yield return Supervising;
            yield return Unknown;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

}
