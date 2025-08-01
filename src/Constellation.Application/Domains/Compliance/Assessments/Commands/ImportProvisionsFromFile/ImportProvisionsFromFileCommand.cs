namespace Constellation.Application.Domains.Compliance.Assessments.Commands.ImportProvisionsFromFile;

using Abstractions.Messaging;
using System.IO;

public sealed record ImportProvisionsFromFileCommand(
    MemoryStream ImportFile)
    : ICommand;