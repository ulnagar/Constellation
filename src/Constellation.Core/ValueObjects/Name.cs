﻿namespace Constellation.Core.ValueObjects;

using Constellation.Core.Errors;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System.Collections.Generic;

public sealed class Name : ValueObject
{
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
            preferredName = string.Empty;
        }

        return new Name(
            firstName.Trim(),
            preferredName?.Trim(),
            lastName.Trim());
    }

    public string FirstName { get; }
    public string PreferredName { get; }
    public string LastName { get; }
    public string DisplayName => $"{(string.IsNullOrEmpty(PreferredName) ? FirstName : PreferredName)} {LastName}";

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return FirstName;
        yield return PreferredName;
        yield return LastName;
    }
}