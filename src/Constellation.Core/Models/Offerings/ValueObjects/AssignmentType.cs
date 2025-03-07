﻿namespace Constellation.Core.Models.Offerings.ValueObjects;

using Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public sealed class AssignmentType : ValueObject
{
    public static readonly AssignmentType ClassroomTeacher = new("Classroom Teacher");
    public static readonly AssignmentType Supervisor = new("Supervisor");
    public static readonly AssignmentType SupportTeacher = new("Support Teacher");
    public static readonly AssignmentType PracTeacher = new("Prac Teacher");
    public static readonly AssignmentType TutorialTeacher = new("Tutorial Teacher");

    public static AssignmentType? FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        
        // Only return the object if it is an existing, defined value above
        if (Enumerations().Any(entry => ((AssignmentType)entry).Value == value))
            return new(value);

        return null;
    }

    private AssignmentType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(AssignmentType? assignmentType) =>
        assignmentType is null ? string.Empty : assignmentType.ToString();

    public static IEnumerable<object> Enumerations()
    {
        Type enumerationType = typeof(AssignmentType);

        IEnumerable<AssignmentType> fieldsForType = enumerationType
            .GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy)
            .Where(fieldInfo =>
                enumerationType.IsAssignableFrom(fieldInfo.FieldType))
            .Select(fieldInfo =>
                (AssignmentType)fieldInfo.GetValue(default)!);

        foreach (AssignmentType? field in fieldsForType)
            yield return field;
    }
}