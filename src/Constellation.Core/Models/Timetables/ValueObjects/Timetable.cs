namespace Constellation.Core.Models.Timetables.ValueObjects;

using Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class Timetable : ValueObject, IComparable<Timetable>
{
    private static readonly Dictionary<string, Timetable> _enumerations = CreateEnumerations();

    public static Timetable Primary => new("PRI", "Primary", 'P');
    public static Timetable Junior6 => new("JU6", "Secondary", 'J');
    public static Timetable Junior8 => new("JU8", "Alternate", 'A');
    public static Timetable Senior => new("SEN", "Senior", 'S');

    private Timetable() { }

    private Timetable(string code, string name, char prefix)
    {
        Code = code;
        Name = name;
        Prefix = prefix;
    }

    public string Code { get; }
    public string Name { get; }
    public char Prefix { get; }
    public string DisplayName => $"{Code} {Name}";

    public static Timetable FromValue(string value) =>
        _enumerations.GetValueOrDefault(value);

    public static Timetable? FromPrefix(char value)
    {
        if (value is '\0')
            return Junior6;

        foreach (KeyValuePair<string, Timetable> timetable in _enumerations)
        {
            if (timetable.Value.Prefix == value)
                return timetable.Value;
        }

        return null;
    }

    public static IEnumerable<Timetable> GetEnumerable = _enumerations
        .Select(entry => entry.Value)
        .AsEnumerable();

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
        yield return Name;
        yield return Prefix;
    }

    public override string ToString() => DisplayName;

    public int CompareTo(object obj)
    {
        if (obj is Timetable other)
        {
            return string.Compare(Code, other.Code, StringComparison.Ordinal);
        }

        return -1;
    }

    public static implicit operator string(Timetable timetable) =>
        timetable is null ? string.Empty : timetable.ToString();

    public bool Equals(Timetable other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return base.Equals(other) &&
               Code == other.Code;
    }

    public int CompareTo(Timetable other) => 
        string.Compare(Code, other.Code, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object obj) =>
        ReferenceEquals(this, obj) || obj is Timetable other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(base.GetHashCode(), Code);

    private static Dictionary<string, Timetable> CreateEnumerations()
    {
        Type enumerationType = typeof(Timetable);

        IEnumerable<Timetable> fieldsForType = enumerationType
            .GetProperties(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy)
            .Where(fieldInfo =>
                enumerationType.IsAssignableFrom(fieldInfo.PropertyType))
            .Select(fieldInfo =>
                (Timetable)fieldInfo.GetValue(default)!);

        return fieldsForType.ToDictionary(x => x.Code);
    }
}