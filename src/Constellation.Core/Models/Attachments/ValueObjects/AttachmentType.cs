namespace Constellation.Core.Models.Attachments.ValueObjects;

using Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public sealed class AttachmentType : ValueObject, IComparable
{
    public static readonly AttachmentType CanvasAssignmentSubmission = new("Canvas Assignment Submission");
    public static readonly AttachmentType StudentReport = new("Student Report");
    public static readonly AttachmentType TrainingCertificate = new("Training Certificate");
    public static readonly AttachmentType AwardCertificate = new("Award Certificate");
    public static readonly AttachmentType WorkFlowEmailAttachment = new("WorkFlow Email Attachment");

    public static AttachmentType FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return new(value);
    }

    private AttachmentType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public int CompareTo(object obj)
    {
        if (obj is AttachmentType other)
        {
            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }

        return -1;
    }

    public static implicit operator string(AttachmentType assignmentType) =>
        assignmentType is null ? string.Empty : assignmentType.ToString();

    public static IEnumerable<object> Enumerations()
    {
        var enumerationType = typeof(AttachmentType);

        var fieldsForType = enumerationType
            .GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy)
            .Where(fieldInfo =>
                enumerationType.IsAssignableFrom(fieldInfo.FieldType))
            .Select(fieldInfo =>
                (AttachmentType)fieldInfo.GetValue(default)!);

        foreach (var field in fieldsForType)
            yield return field;
    }
}