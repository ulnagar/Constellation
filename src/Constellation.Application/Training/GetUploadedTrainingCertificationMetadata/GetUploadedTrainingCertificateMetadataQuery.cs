namespace Constellation.Application.Training.GetUploadedTrainingCertificationMetadata;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetUploadedTrainingCertificateMetadataQuery(
    string LinkType,
    string LinkId)
    : IQuery<CompletionRecordCertificateDto>;
