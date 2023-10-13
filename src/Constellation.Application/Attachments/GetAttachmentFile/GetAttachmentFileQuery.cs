namespace Constellation.Application.Attachments.GetAttachmentFile;

using Abstractions.Messaging;
using Core.Models.Attachments.ValueObjects;

public sealed record GetAttachmentFileQuery(
        AttachmentType Type,
        string LinkId)
    : IQuery<AttachmentResponse>;