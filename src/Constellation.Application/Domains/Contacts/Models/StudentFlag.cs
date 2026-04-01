namespace Constellation.Application.Domains.Contacts.Models;

using Core.Primitives;

public sealed class StudentFlag : ValueObject, IComparable<StudentFlag>, IComparable
{
    public StudentFlag(
        string flag)
    {
        Name = flag;
    }

    public string Name { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Name;
    }

    public static StudentFlag FromValue(string value) => new(value);

    public override string ToString() => Name;
    
    public int CompareTo(object? obj)
    {
        if (obj is StudentFlag other)
            return CompareTo(other);

        throw new ArgumentException("Object is not a StudentFlag instance", nameof(obj));
    }

    public int CompareTo(StudentFlag? other) =>
        string.Compare(Name, other.Name, StringComparison.Ordinal);

}
