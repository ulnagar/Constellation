namespace Constellation.Application.Domains.GroupTutorials.Queries.GenerateTutorialAttendanceReport;

using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record TutorialRollDetailsDto(
    TutorialRollId Id,
    DateOnly SessionDate,
    string StaffId,
    string StaffName,
    IReadOnlyCollection<TutorialRollStudentDetailsDto> Students);
