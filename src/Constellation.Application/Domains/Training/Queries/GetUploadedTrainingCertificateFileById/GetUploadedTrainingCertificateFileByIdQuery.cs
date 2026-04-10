namespace Constellation.Application.Domains.Training.Queries.GetUploadedTrainingCertificateFileById;

using Abstractions.Messaging;
using Core.Models.Attachments.Enums;

public sealed record GetUploadedTrainingCertificateFileByIdQuery(
    AttachmentType LinkType,
    string LinkId)
    : IQuery<CompletionRecordCertificateDetailsDto>;
