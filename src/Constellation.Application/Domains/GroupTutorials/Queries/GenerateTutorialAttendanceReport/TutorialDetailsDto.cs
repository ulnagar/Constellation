namespace Constellation.Application.Domains.GroupTutorials.Queries.GenerateTutorialAttendanceReport;

using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record TutorialDetailsDto(
    GroupTutorialId Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyCollection<TutorialRollDetailsDto> Rolls);
