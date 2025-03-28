namespace Constellation.Core.Models.Prospects.Identifiers;

using Primitives;
using System;

public record struct ProspectiveStudentId(Guid Value)
    : IStronglyTypedId
{
    public static ProspectiveStudentId Empty => new(Guid.Empty);

    public static ProspectiveStudentId FromValue(Guid value) =>
        new(value);

    public ProspectiveStudentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}