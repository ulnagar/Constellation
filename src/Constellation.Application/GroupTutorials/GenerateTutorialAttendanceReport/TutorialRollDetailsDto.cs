namespace Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;

using System;
using System.Collections.Generic;

public sealed record TutorialRollDetailsDto(
    Guid Id,
    DateOnly SessionDate,
    string StaffId,
    string StaffName,
    IReadOnlyCollection<TutorialRollStudentDetailsDto> Students);
