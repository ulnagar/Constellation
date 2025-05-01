namespace Constellation.Application.Domains.Training.Queries.GetUploadedTrainingCertificateFileById;

using Abstractions.Messaging;

public sealed record GetUploadedTrainingCertificateFileByIdQuery(
    string LinkType,
    string LinkId)
    : IQuery<CompletionRecordCertificateDetailsDto>;
