namespace Constellation.Application.MandatoryTraining.GetUploadedTrainingCertificationMetadata;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetUploadedTrainingCertificateMetadataQuery(
    string LinkType,
    string LinkId)
    : IQuery<CompletionRecordCertificateDto>;
