﻿namespace Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Identifiers;

public sealed record GenerateTutorialAttendanceReportQuery(
    GroupTutorialId TutorialId) 
    : IQuery<FileDto>;
