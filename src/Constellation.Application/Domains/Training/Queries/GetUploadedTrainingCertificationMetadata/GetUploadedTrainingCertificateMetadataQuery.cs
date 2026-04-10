namespace Constellation.Application.Domains.Training.Queries.GetUploadedTrainingCertificationMetadata;

using Abstractions.Messaging;
using Core.Models.Attachments.Enums;

public sealed record GetUploadedTrainingCertificateMetadataQuery(
    AttachmentType LinkType,
    string LinkId)
    : IQuery<CompletionRecordCertificateDto>;
