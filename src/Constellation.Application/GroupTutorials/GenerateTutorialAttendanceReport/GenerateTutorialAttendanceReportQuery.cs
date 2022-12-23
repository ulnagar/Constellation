namespace Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using System;

public sealed record GenerateTutorialAttendanceReportQuery(
    Guid TutorialId) : IQuery<FileDto>;
