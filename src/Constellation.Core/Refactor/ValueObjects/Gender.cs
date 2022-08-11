namespace Constellation.Core.Refactor.ValueObjects;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

public class Gender : ValueObject
{
    static Gender() { }

    private Gender() { }

    private Gender(string code)
    {
        Code = code;
    }

    public static Gender From(string code)
    {
        var gender = new Gender { Code = code };

        if (!SupportedGenders.Contains(gender))
        {
            throw new UnsupportedGenderException(code);
        }

        return gender;
    }

    public static Gender Male => new("Male");
    public static Gender Female => new("Female");
    public static Gender NonBinary => new("Non Binary");
    public static Gender Unknown => new("Unknown");
    public string Code { get; private set; } = "Unknown";

    public static implicit operator string(Gender gender)
    {
        return gender.ToString();
    }

    public static explicit operator Gender(string code)
    {
        return From(code);
    }

    public override string ToString()
    {
        return Code;
    }

    protected static IEnumerable<Gender> SupportedGenders
    {
        get
        {
            yield return Male;
            yield return Female;
            yield return NonBinary;
            yield return Unknown;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}
