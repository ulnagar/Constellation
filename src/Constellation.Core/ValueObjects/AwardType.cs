﻿namespace Constellation.Core.ValueObjects;

using Constellation.Core.Primitives;
using System.Collections.Generic;

public sealed class AwardType : ValueObject
{
    public static readonly AwardType FirstInSubject = new("First in Subject");
    public static readonly AwardType AcademicAchievement = new("Academic Achievement");
    public static readonly AwardType AcademicExcellence = new("Academic Excellence");
    public static readonly AwardType PrincipalsAward = new("Principals Award");
    public static readonly AwardType GalaxyMedal = new("Galaxy Medal");
    public static readonly AwardType UniversalAchiever = new("Universal Achiever");

    public static AwardType FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return new(value);
    }

    private AwardType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}