namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct StudentAbsenceConfigurationId(Guid Value)
    : IStronglyTypedId
{
    public static StudentAbsenceConfigurationId FromValue(Guid Value) =>
        new(Value);

    public StudentAbsenceConfigurationId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}