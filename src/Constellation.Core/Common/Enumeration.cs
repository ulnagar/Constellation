namespace Constellation.Core.Common;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

public abstract class IntEnumeration<TEnum> : IEquatable<IntEnumeration<TEnum>>, IComparable
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

    public int Value { get; }
    public string Name { get; } = string.Empty;

    public static TEnum? FromValue(int value) =>
        Enumerations.GetValueOrDefault(value);

    public static TEnum? FromName(string name) =>
        Enumerations
            .Values
            .SingleOrDefault(e => e.Name == name);

    protected static readonly IEnumerable<TEnum> GetEnumerable = CreateEnumerations()
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
                (TEnum)fieldInfo.GetValue(null)!);

        return fieldsForType.ToDictionary(x => x.Value);
    }

    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1;

        if (obj is not TEnum incomingObject)
            throw new ArgumentException($"Object must be of type {nameof(TEnum)}", nameof(obj));

        return Value.CompareTo(incomingObject.Value);
    }

    private IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    private bool ValuesAreEqual(IntEnumeration<TEnum> other)
    {
        return GetType() == other.GetType() &&
            GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    private static bool EqualOperator(IntEnumeration<TEnum>? left, IntEnumeration<TEnum>? right)
    {
        if (left is null ^ right is null)
            return false;

        if (left is null & right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.ValuesAreEqual(right);
    }

    private static bool NotEqualOperator(IntEnumeration<TEnum>? left, IntEnumeration<TEnum>? right)
    {
        return !(EqualOperator(left, right));
    }

    public static bool operator ==(IntEnumeration<TEnum>? left, IntEnumeration<TEnum>? right)
    {
        return EqualOperator(left, right);
    }

    public static bool operator !=(IntEnumeration<TEnum>? left, IntEnumeration<TEnum>? right)
    {
        return NotEqualOperator(left, right);
    }

    public static bool operator <(IntEnumeration<TEnum> left, IntEnumeration<TEnum> right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(IntEnumeration<TEnum> left, IntEnumeration<TEnum> right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(IntEnumeration<TEnum> left, IntEnumeration<TEnum> right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(IntEnumeration<TEnum> left, IntEnumeration<TEnum> right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
}

public abstract class StringEnumeration<TEnum> : IEquatable<StringEnumeration<TEnum>>, IComparable
    where TEnum : StringEnumeration<TEnum>
{
    private static readonly Dictionary<string, TEnum> _enumerations = CreateEnumerations();
    
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

    protected static readonly IEnumerable<TEnum> GetEnumerable = CreateEnumerations()
        .Select(entry => entry.Value)
        .Where(entry => !string.IsNullOrWhiteSpace(entry.Value))
        .AsEnumerable();

    public string Value { get; protected init; }
    public string Name { get; protected init; }
    public int Order { get; }

    public static TEnum? FromValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return _enumerations.GetValueOrDefault(value);
    }

    public static TEnum? FromName(string name) =>
        _enumerations
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

    public override int GetHashCode() => Value.GetHashCode(StringComparison.InvariantCulture);

    public override string ToString() => Name;

    public int CompareTo(object? obj)
    {
        if (obj is not TEnum incomingObject)
            return -1;

        return incomingObject.Order == 0 
            ? string.Compare(Value, incomingObject.Value, StringComparison.OrdinalIgnoreCase) 
            : string.CompareOrdinal(Order.ToString(CultureInfo.InvariantCulture), incomingObject.Order.ToString(CultureInfo.InvariantCulture));
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
                (TEnum)fieldInfo.GetValue(null)!);

        return fieldsForType.ToDictionary(x => x.Value);
    }

    private IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    private bool ValuesAreEqual(StringEnumeration<TEnum> other)
    {
        return GetAtomicValues()
            .SequenceEqual(other.GetAtomicValues());
    }

    private static bool EqualOperator(StringEnumeration<TEnum>? left, StringEnumeration<TEnum>? right)
    {
        if (left is null ^ right is null)
            return false;

        if (left is null & right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.ValuesAreEqual(right);
    }

    private static bool NotEqualOperator(StringEnumeration<TEnum> left, StringEnumeration<TEnum> right)
    {
        return !(EqualOperator(left, right));
    }

    public static bool operator ==(StringEnumeration<TEnum> left, StringEnumeration<TEnum> right)
    {
        return EqualOperator(left, right);
    }

    public static bool operator !=(StringEnumeration<TEnum> left, StringEnumeration<TEnum> right)
    {
        return NotEqualOperator(left, right);
    }

    public static bool operator <(StringEnumeration<TEnum> left, StringEnumeration<TEnum> right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(StringEnumeration<TEnum> left, StringEnumeration<TEnum> right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(StringEnumeration<TEnum> left, StringEnumeration<TEnum> right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(StringEnumeration<TEnum> left, StringEnumeration<TEnum> right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
}