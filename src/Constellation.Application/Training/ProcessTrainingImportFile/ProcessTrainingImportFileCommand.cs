namespace Constellation.Application.Training.ProcessTrainingImportFile;

using Constellation.Application.Abstractions.Messaging;
using System.IO;

public sealed record ProcessTrainingImportFileCommand(
    MemoryStream Stream)
    : ICommand;
