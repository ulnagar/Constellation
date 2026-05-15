namespace Constellation.Core.Models.Attachments.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct AttachmentId(Guid Value)
    : IStronglyTypedId<AttachmentId, Guid>
{
    public static AttachmentId Empty => new(Guid.Empty);
    public static AttachmentId FromValue(Guid value) =>
        new(value);

    public AttachmentId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}