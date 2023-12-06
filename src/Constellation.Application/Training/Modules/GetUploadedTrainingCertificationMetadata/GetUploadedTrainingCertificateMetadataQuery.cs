namespace Constellation.Application.Training.Modules.GetUploadedTrainingCertificationMetadata;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetUploadedTrainingCertificateMetadataQuery(
    string LinkType,
    string LinkId)
    : IQuery<CompletionRecordCertificateDto>;
