namespace Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;

using System;
using System.Collections.Generic;

public sealed record TutorialDetailsDto(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyCollection<TutorialRollDetailsDto> Rolls);
