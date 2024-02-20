namespace Constellation.Core.Models.SchoolContacts.Identifiers;

using System;

public sealed record SchoolContactId(Guid Value)
{
    public static SchoolContactId FromValue(Guid value) =>
        new(value);

    public SchoolContactId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}