namespace Constellation.Core.ValueObjects;

using Constellation.Core.Primitives;
using Errors;
using Shared;
using System;
using System.Collections.Generic;

public sealed class MailingAddress : ValueObject, IComparable, IEquatable<MailingAddress>
{
    private MailingAddress() { }

    private MailingAddress(
        string title, 
        string line1, 
        string line2, 
        string town, 
        string state, 
        string postcode)
    {
        Title = title;
        Line1 = line1;
        Line2 = line2;
        Town = town;
        State = state;
        PostCode = postcode;
    }

    public static Result<MailingAddress> Create(
        string title,
        string line1,
        string? line2,
        string town,
        string state,
        string postcode)
    {
        if (string.IsNullOrEmpty(title))
            return Result.Failure<MailingAddress>(MailingAddressErrors.TitleEmpty);

        if (string.IsNullOrEmpty(line1))
            return Result.Failure<MailingAddress>(MailingAddressErrors.Line1Empty);

        if (string.IsNullOrWhiteSpace(town))
            return Result.Failure<MailingAddress>(MailingAddressErrors.TownEmpty);

        if (string.IsNullOrWhiteSpace(state))
            return Result.Failure<MailingAddress>(MailingAddressErrors.StateEmpty);

        if (string.IsNullOrWhiteSpace(postcode))
            return Result.Failure<MailingAddress>(MailingAddressErrors.PostCodeEmpty);

        return new MailingAddress(
            title,
            line1,
            line2 ?? string.Empty,
            town,
            state,
            postcode);
    }
    
    // EF Core use only
    public static MailingAddress FromValue(string title,
        string line1,
        string? line2,
        string town,
        string state,
        string postcode) => 
        new MailingAddress(
            title,
            line1,
            line2 ?? string.Empty,
            town,
            state,
            postcode);

    public string Title { get; }
    public string Line1 { get; }
    public string Line2 { get; }
    public string Town { get; }
    public string State { get; }
    public string PostCode { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Title;
        yield return Line1;
        yield return Line2;
        yield return Town;
        yield return State;
        yield return PostCode;
    }

    public int CompareTo(object? obj)
    {
        if (obj is MailingAddress other)
        {
            return string.Compare(ToString(), other.ToString(), StringComparison.Ordinal);
        }

        return -1;
    }

    public bool Equals(MailingAddress? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return base.Equals(other)
               && Title == other.Title
               && Line1 == other.Line1
               && Line2 == other.Line2
               && Town == other.Town
               && State == other.State
               && PostCode == other.PostCode;
    }

    public override int GetHashCode() =>
        HashCode.Combine(base.GetHashCode(), Title, Line1, Line2, Town, State, PostCode);
}