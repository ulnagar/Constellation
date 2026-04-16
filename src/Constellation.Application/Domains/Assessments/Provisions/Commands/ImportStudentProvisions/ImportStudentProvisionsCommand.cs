namespace Constellation.Application.Domains.Assessments.Provisions.Commands.ImportStudentProvisions;

using Abstractions.Messaging;

public sealed record ImportStudentProvisionsCommand(
    MemoryStream ImportFile)
    : ICommand<List<string>>;