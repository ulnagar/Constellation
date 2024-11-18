#nullable enable
namespace Constellation.Core.ValueObjects;

using Enums;
using Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public sealed class AwardType : ValueObject
{
    public static readonly AwardType FirstInSubject = new("First in Course");
    public static readonly AwardType FirstInSubjectMathematics = new("First in Course - Mathematics", [Grade.Y05, Grade.Y06]);
    public static readonly AwardType FirstInSubjectScienceTechnology = new("First in Course - Science & Technology", [Grade.Y05, Grade.Y06]);
    public static readonly AwardType AcademicAchievement = new("Academic Achievement");
    public static readonly AwardType AcademicAchievementMathematics = new("Academic Achievement - Mathematics", [Grade.Y05, Grade.Y06]);
    public static readonly AwardType AcademicAchievementScienceTechnology = new("Academic Achievement - Science & Technology", [Grade.Y05, Grade.Y06]);
    public static readonly AwardType AcademicExcellence = new("Academic Excellence");
    public static readonly AwardType AcademicExcellenceMathematics = new("Academic Excellence - Mathematics", [Grade.Y05, Grade.Y06]);
    public static readonly AwardType AcademicExcellenceScienceTechnology = new("Academic Excellence - Science & Technology", [Grade.Y05, Grade.Y06]);
    public static readonly AwardType PrincipalsAward = new("Principals Award");
    public static readonly AwardType GalaxyMedal = new("Galaxy Medal");
    public static readonly AwardType UniversalAchiever = new("Universal Achiever");

    public static AwardType? FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        IEnumerable<AwardType> defined = CreateEnumerations()
            .Where(entry => entry.Key == value)
            .Select(entry => entry.Value)
            .ToList();

        if (!defined.Any() || defined.Count() > 1)
            return null;

        return defined.First();
    }

    private AwardType(string value)
    {
        Value = value;
        Grades = new();
    }

    private AwardType(string value, List<Grade> grades)
    {
        Value = value;
        Grades = grades;
    }

    public string Value { get; }
    public List<Grade> Grades { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static readonly IEnumerable<AwardType> Options = CreateEnumerations()
        .Select(entry => entry.Value)
        .AsEnumerable();

    private static Dictionary<string, AwardType> CreateEnumerations()
    {
        Type enumerationType = typeof(AwardType);

        IEnumerable<AwardType> fieldsForType = enumerationType
            .GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy)
            .Where(fieldInfo =>
                enumerationType.IsAssignableFrom(fieldInfo.FieldType))
            .Select(fieldInfo =>
                (AwardType)fieldInfo.GetValue(default)!);

        return fieldsForType.ToDictionary(x => x.Value);
    }
}