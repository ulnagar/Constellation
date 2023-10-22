namespace Constellation.Core.Models.Attachments.Identifiers;

using System;

public sealed record AttachmentId(Guid Value)
{
    public static AttachmentId FromValue(Guid value) =>
        new(value);

    public AttachmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}