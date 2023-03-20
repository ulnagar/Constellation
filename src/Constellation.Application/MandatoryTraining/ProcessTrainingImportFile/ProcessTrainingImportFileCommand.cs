namespace Constellation.Application.MandatoryTraining.ProcessTrainingImportFile;

using Constellation.Application.Abstractions.Messaging;
using System.IO;

public sealed record ProcessTrainingImportFileCommand(
    MemoryStream Stream)
    : ICommand;
