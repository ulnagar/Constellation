namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public sealed record StudentAbsenceConfigurationId(Guid Value)
    : IStronglyTypedId
{
    public static StudentAbsenceConfigurationId FromValue(Guid Value) =>
        new(Value);

    public StudentAbsenceConfigurationId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}