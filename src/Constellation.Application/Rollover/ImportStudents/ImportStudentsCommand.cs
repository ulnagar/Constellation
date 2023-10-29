namespace Constellation.Application.Rollover.ImportStudents;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record ImportStudentsCommand(
    List<StudentImportRecord> Records)
    : ICommand<List<ImportResult>>;