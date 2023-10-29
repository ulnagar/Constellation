namespace Constellation.Application.Rollover.ImportStudents;

using Core.Shared;

public sealed record ImportResult(
    StudentImportRecord Record,
    Result Result);