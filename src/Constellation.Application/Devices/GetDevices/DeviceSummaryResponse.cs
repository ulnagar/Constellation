namespace Constellation.Application.Devices.GetDevices;

using Core.Enums;

public sealed record DeviceSummaryResponse(
    string SerialNumber,
    string Make,
    Status Status,
    string? StudentName,
    Grade? StudentGrade,
    string? StudentSchool);
