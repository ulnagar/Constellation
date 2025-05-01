namespace Constellation.Application.Domains.Attachments.Queries.GetListOfAttachments;

using Abstractions.Messaging;
using Core.Models.Attachments;
using System.Collections.Generic;

public sealed record GetListOfAttachmentsQuery()
    : IQuery<List<Attachment>>;