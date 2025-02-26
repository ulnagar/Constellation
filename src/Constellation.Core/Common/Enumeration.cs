#nullable enable
namespace Constellation.Core.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public abstract class IntEnumeration<TEnum> : IEquatable<IntEnumeration<TEnum>>
    where TEnum : IntEnumeration<TEnum>
{
    protected static readonly Dictionary<int, TEnum> Enumerations = CreateEnumerations();

    /// <summary>
    /// Do not use. For serialization purposes only.
    /// </summary>
    protected IntEnumeration() { }

    protected IntEnumeration(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public int Value { get; protected init; }
    public string Name { get; protected init; } = string.Empty;

    public static TEnum? FromValue(int value) =>
        Enumerations.GetValueOrDefault(value);

    public static TEnum? FromName(string name) =>
        Enumerations
            .Values
            .SingleOrDefault(e => e.Name == name);

    public static IEnumerable<TEnum> GetEnumerable = CreateEnumerations()
        .Select(entry => entry.Value)
        .AsEnumerable();

    public bool Equals(IntEnumeration<TEnum>? other)
    {
        if (other is null)
            return false;

        return GetType() == other.GetType() &&
            Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        IntEnumeration<TEnum>? other = obj as IntEnumeration<TEnum>;
        return other != null &&
               Equals(other);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Name;

    private static Dictionary<int, TEnum> CreateEnumerations()
    {
        Type enumerationType = typeof(TEnum);

        IEnumerable<TEnum> fieldsForType = enumerationType
            .GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy)
            .Where(fieldInfo =>
                enumerationType.IsAssignableFrom(fieldInfo.FieldType))
            .Select(fieldInfo =>
                (TEnum)fieldInfo.GetValue(default)!);

        return fieldsForType.ToDictionary(x => x.Value);
    }
}

public abstract class StringEnumeration<TEnum> : IEquatable<StringEnumeration<TEnum>>, IComparable
    where TEnum : StringEnumeration<TEnum>
{
    private static readonly Dictionary<string, TEnum> Enumerations = CreateEnumerations();

    protected StringEnumeration(string value, string name)
    {
        Value = value;
        Name = name;
    }

    protected StringEnumeration(string value, string name, int order)
    {
        Value = value;
        Name = name;
        Order = order;
    }

    public static IEnumerable<TEnum> GetEnumerable = CreateEnumerations()
        .Select(entry => entry.Value)
        .AsEnumerable();

    public string Value { get; protected init; }
    public string Name { get; protected init; }
    public int Order { get; protected init; }

    public static TEnum? FromValue(string value) =>
        Enumerations.GetValueOrDefault(value);

    public static TEnum? FromName(string name) =>
        Enumerations
            .Values
            .SingleOrDefault(e => e.Name == name);

    public bool Equals(StringEnumeration<TEnum>? other)
    {
        if (other is null)
            return false;

        return GetType() == other.GetType() &&
            Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        StringEnumeration<TEnum>? other = obj as StringEnumeration<TEnum>;
        return other is not null &&
               Equals(other);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Name;

    public int CompareTo(object? obj)
    {
        if (obj is not TEnum incomingObject)
            return -1;

        return incomingObject?.Order == 0 
            ? string.Compare(Value, incomingObject?.Value, StringComparison.OrdinalIgnoreCase) 
            : string.CompareOrdinal(Order.ToString(), incomingObject?.Order.ToString());
    }

    private static Dictionary<string, TEnum> CreateEnumerations()
    {
        Type enumerationType = typeof(TEnum);

        IEnumerable<TEnum> fieldsForType = enumerationType
            .GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy)
            .Where(fieldInfo =>
                enumerationType.IsAssignableFrom(fieldInfo.FieldType))
            .Select(fieldInfo =>
                (TEnum)fieldInfo.GetValue(default)!);

        return fieldsForType.ToDictionary(x => x.Value);
    }
}