namespace Constellation.Core.Models.SchoolContacts.Identifiers;

using System;

public sealed record SchoolContactRoleId(Guid Value)
{
    public static SchoolContactRoleId FromValue(Guid value) =>
        new(value);

    public SchoolContactRoleId()
        : this(Guid.NewGuid()) { }

    public override string ToString() => 
        Value.ToString();
}