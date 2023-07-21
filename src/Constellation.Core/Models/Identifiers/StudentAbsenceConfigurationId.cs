namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record StudentAbsenceConfigurationId(Guid Value)
{
    public static StudentAbsenceConfigurationId FromValue(Guid Value) =>
        new(Value);

    public StudentAbsenceConfigurationId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}