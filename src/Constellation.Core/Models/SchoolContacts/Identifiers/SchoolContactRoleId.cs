namespace Constellation.Core.Models.SchoolContacts.Identifiers;

using System;

public readonly record struct SchoolContactRoleId(Guid Value)
{
    public static SchoolContactRoleId Empty => new(Guid.Empty);

    public static SchoolContactRoleId FromValue(Guid value) =>
        new(value);

    public SchoolContactRoleId()
        : this(Guid.NewGuid()) { }

    public override string ToString() => 
        Value.ToString();
}