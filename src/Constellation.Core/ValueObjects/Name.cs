namespace Constellation.Core.ValueObjects;

using Errors;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;

public sealed class Name : ValueObject, IComparable, IEquatable<Name>
{
    private Name() { }

    private Name(string firstName, string preferredName, string lastName)
    {
        FirstName = firstName;
        PreferredName = preferredName;
        LastName = lastName;
    }

    public static Result<Name> Create(string firstName, string preferredName, string lastName)
    {
        if (string.IsNullOrEmpty(firstName))
        {
            return Result.Failure<Name>(DomainErrors.ValueObjects.Name.FirstNameEmpty);
        }

        if (string.IsNullOrEmpty(lastName))
        {
            return Result.Failure<Name>(DomainErrors.ValueObjects.Name.LastNameEmpty);
        }

        if (string.IsNullOrEmpty(preferredName))
        {
            preferredName = firstName;
        }

        return new Name(
            firstName.Trim(),
            preferredName.Trim(),
            lastName.Trim());
    }
    
    public string FirstName { get; }
    public string PreferredName { get; }
    public string LastName { get; }
    public string DisplayName => $"{PreferredName} {LastName}";
    public string SortOrder => $"{LastName}, {PreferredName}";

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return FirstName;
        yield return PreferredName;
        yield return LastName;
    }

    public override string ToString() => DisplayName;

    public int CompareTo(object obj)
    {
        if (obj is Name other)
        {
            return string.Compare(SortOrder, other.SortOrder, StringComparison.Ordinal);
        }

        return -1;
    }

    public static implicit operator string(Name name) =>
        name is null ? string.Empty : name.ToString();

    public bool Equals(Name other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return base.Equals(other) && 
               FirstName == other.FirstName && 
               PreferredName == other.PreferredName && 
               LastName == other.LastName;
    }

    public override bool Equals(object obj) => 
        ReferenceEquals(this, obj) || obj is Name other && Equals(other);

    public override int GetHashCode() => 
        HashCode.Combine(base.GetHashCode(), FirstName, PreferredName, LastName);
}
