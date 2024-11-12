namespace Constellation.Application.Students.ImportStudentsFromFile;

using Constellation.Application.Abstractions.Messaging;
using DTOs;
using System.Collections.Generic;
using System.IO;

public sealed record ImportStudentsFromFileCommand(
    MemoryStream ImportFile,
    bool RemoveExcess = false)
    : ICommand<List<ImportStatusDto>>;