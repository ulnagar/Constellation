namespace Constellation.Core.Models.Faculties.ValueObjects;

using Constellation.Core.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public sealed class FacultyMembershipRole : ValueObject
{
    public static readonly FacultyMembershipRole Member = new("Member");
    public static readonly FacultyMembershipRole Approver = new("Approver");
    public static readonly FacultyMembershipRole Manager = new("Manager");

    public static FacultyMembershipRole FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return new(value);
    }

    private FacultyMembershipRole(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(FacultyMembershipRole assignmentType) =>
        assignmentType is null ? string.Empty : assignmentType.ToString();

    public static IEnumerable<object> Enumerations()
    {
        var enumerationType = typeof(FacultyMembershipRole);

        var fieldsForType = enumerationType
            .GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy)
            .Where(fieldInfo =>
                enumerationType.IsAssignableFrom(fieldInfo.FieldType))
            .Select(fieldInfo =>
                (FacultyMembershipRole)fieldInfo.GetValue(default)!);

        foreach (var field in fieldsForType)
            yield return field;
    }
}