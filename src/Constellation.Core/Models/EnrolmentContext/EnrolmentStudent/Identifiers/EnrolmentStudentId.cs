namespace Constellation.Core.Models.EnrolmentContext.EnrolmentStudent.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct EnrolmentStudentId(Guid Value)
    : IStronglyTypedId<EnrolmentStudentId, Guid>
{
    public static EnrolmentStudentId Empty => new(Guid.Empty);

    public static EnrolmentStudentId FromValue(Guid value) =>
        new(value);

    public EnrolmentStudentId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();

}