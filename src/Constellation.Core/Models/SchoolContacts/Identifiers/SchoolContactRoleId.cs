namespace Constellation.Core.Models.SchoolContacts.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct SchoolContactRoleId(Guid Value)
    : IStronglyTypedId<SchoolContactRoleId, Guid>
{
    public static SchoolContactRoleId Empty => new(Guid.Empty);

    public static SchoolContactRoleId FromValue(Guid value) =>
        new(value);

    public SchoolContactRoleId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() => 
        Value.ToString();
}