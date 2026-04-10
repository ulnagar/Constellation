namespace Constellation.Core.Models.Assessments.Identifiers;

using Primitives;

public readonly record struct StudentProvisionId(Guid Value)
    : IStronglyTypedId<StudentProvisionId, Guid>
{
    public static StudentProvisionId Empty => new(Guid.Empty);
    public static StudentProvisionId FromValue(Guid value) =>
        new(value);
    public StudentProvisionId()
        : this(Guid.CreateVersion7()) { }
    public override string ToString() =>
        Value.ToString();
}