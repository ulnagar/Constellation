namespace Constellation.Application.Training.Modules.GetUploadedTrainingCertificateFileById;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetUploadedTrainingCertificateFileByIdQuery(
    string LinkType,
    string LinkId)
    : IQuery<CompletionRecordCertificateDetailsDto>;
