namespace Constellation.Application.DTOs.CSV;

using Constellation.Core.Enums;

public sealed record MasterFileStudent(
    string SRN,
    string FirstName,
    string LastName,
    Grade Grade,
    string Parent1Email,
    string Parent2Email);