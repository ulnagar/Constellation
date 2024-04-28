#nullable enable
namespace Constellation.Core.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public abstract class IntEnumeration<TEnum> : IEquatable<IntEnumeration<TEnum>>
    where TEnum : IntEnumeration<TEnum>
{
    private static readonly Dictionary<int, TEnum> Enumerations = CreateEnumerations();

    protected IntEnumeration(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public int Value { get; protected init; }
    public string Name { get; protected init; } = string.Empty;

    public static TEnum? FromValue(int value)
    {
        return Enumerations.TryGetValue(
            value,
            out TEnum? enumeration) ?
                enumeration :
                default;
    }

    public static TEnum? FromName(string name)
    {
        return Enumerations
            .Values
            .SingleOrDefault(e => e.Name == name);
    }

    public bool Equals(IntEnumeration<TEnum> other)
    {
        if (other is null)
            return false;

        return GetType() == other.GetType() &&
            Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is IntEnumeration<TEnum> other &&
            Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }

    private static Dictionary<int, TEnum> CreateEnumerations()
    {
        var enumerationType = typeof(TEnum);

        var fieldsForType = enumerationType
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

    public string Value { get; protected init; } = string.Empty;
    public string Name { get; protected init; } = string.Empty;
    public int Order { get; protected init; } = 0;

    public static TEnum? FromValue(string value)
    {
        return Enumerations.TryGetValue(
            value,
            out TEnum? enumeration) ?
                enumeration :
                default;
    }

    public static TEnum? FromName(string name)
    {
        return Enumerations
            .Values
            .SingleOrDefault(e => e.Name == name);
    }

    public bool Equals(StringEnumeration<TEnum> other)
    {
        if (other is null)
            return false;

        return GetType() == other.GetType() &&
            Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is StringEnumeration<TEnum> other &&
            Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }

    public int CompareTo(object? obj)
    {
        TEnum? incomingObject = obj as TEnum;

        if (incomingObject is null)
            return -1;

        if (incomingObject?.Order == 0)
            return string.Compare(this.Value, incomingObject?.Value, StringComparison.OrdinalIgnoreCase);
        else
            return string.CompareOrdinal(this.Order.ToString(), incomingObject?.Order.ToString());
    }

    private static Dictionary<string, TEnum> CreateEnumerations()
    {
        var enumerationType = typeof(TEnum);

        var fieldsForType = enumerationType
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