namespace Constellation.Application.Domains.Attachments.Queries.GetAttachmentFile;

using Abstractions.Messaging;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.ValueObjects;

public sealed record GetAttachmentFileQuery(
        AttachmentType Type,
        string LinkId)
    : IQuery<AttachmentResponse>;