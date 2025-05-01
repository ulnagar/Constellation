namespace Constellation.Application.Domains.Training.Commands.ProcessTrainingImportFile;

using Abstractions.Messaging;
using System.IO;

public sealed record ProcessTrainingImportFileCommand(
    MemoryStream Stream)
    : ICommand;
