namespace Constellation.Application.Domains.Training.Queries.GetUploadedTrainingCertificationMetadata;

using Abstractions.Messaging;

public sealed record GetUploadedTrainingCertificateMetadataQuery(
    string LinkType,
    string LinkId)
    : IQuery<CompletionRecordCertificateDto>;
