namespace Constellation.Core.Models.SchoolContacts.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct SchoolContactId(Guid Value)
    : IStronglyTypedId
{
    public static SchoolContactId Empty => new(Guid.Empty);

    public static SchoolContactId FromValue(Guid value) =>
        new(value);

    public SchoolContactId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}