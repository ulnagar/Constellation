namespace Constellation.Core.Models.SchoolContacts.Identifiers;

using System;

public readonly record struct SchoolContactId(Guid Value)
{
    public static SchoolContactId Empty => new(Guid.Empty);

    public static SchoolContactId FromValue(Guid value) =>
        new(value);

    public SchoolContactId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}