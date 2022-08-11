namespace Constellation.Core.Refactor.ValueObjects;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

public class ClassType : ValueObject
{
    static ClassType() { }

    private ClassType() { }

    private ClassType(string code)
    {
        Code = code;
    }

    public static ClassType From(string code)
    {
        var classType = new ClassType { Code = code };

        if (!SupportedClassTypes.Contains(classType))
        {
            throw new UnsupportedClassTypeException(code);
        }

        return classType;
    }

    public static ClassType Mainstream => new("Mainstream");
    public static ClassType Tutorial => new("Tutorial");
    public static ClassType Extracurricular => new("Extra Curricular");
    public static ClassType Unknown => new("Unknown");
    public string Code { get; private set; } = "Unknown";

    public static implicit operator string(ClassType classType)
    {
        return classType.ToString();
    }

    public static explicit operator ClassType(string code)
    {
        return From(code);
    }

    public override string ToString()
    {
        return Code;
    }

    protected static IEnumerable<ClassType> SupportedClassTypes
    {
        get
        {
            yield return Mainstream;
            yield return Tutorial;
            yield return Extracurricular;
            yield return Unknown;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

}
