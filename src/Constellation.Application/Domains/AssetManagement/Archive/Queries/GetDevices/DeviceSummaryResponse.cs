namespace Constellation.Application.Domains.AssetManagement.Archive.Queries.GetDevices;

using Core.Enums;

public sealed record DeviceSummaryResponse(
    string SerialNumber,
    string Make,
    Status Status,
    string? StudentName,
    Grade? StudentGrade,
    string? StudentSchool);
