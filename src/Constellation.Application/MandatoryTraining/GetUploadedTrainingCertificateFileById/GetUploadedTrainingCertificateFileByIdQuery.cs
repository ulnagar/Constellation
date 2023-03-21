namespace Constellation.Application.MandatoryTraining.GetUploadedTrainingCertificateFileById;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetUploadedTrainingCertificateFileByIdQuery(
    string LinkType,
    string LinkId)
    : IQuery<CompletionRecordCertificateDetailsDto>;
