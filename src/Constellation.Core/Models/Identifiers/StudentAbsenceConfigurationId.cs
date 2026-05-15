namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct StudentAbsenceConfigurationId(Guid Value)
    : IStronglyTypedId<StudentAbsenceConfigurationId, Guid>
{
    public static StudentAbsenceConfigurationId Empty => new(Guid.Empty);

    public static StudentAbsenceConfigurationId FromValue(Guid value) =>
        new(value);

    public StudentAbsenceConfigurationId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}