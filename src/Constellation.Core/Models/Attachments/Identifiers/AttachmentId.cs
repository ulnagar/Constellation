﻿namespace Constellation.Core.Models.Attachments.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct AttachmentId(Guid Value)
    : IStronglyTypedId
{
    public static AttachmentId FromValue(Guid value) =>
        new(value);

    public AttachmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}