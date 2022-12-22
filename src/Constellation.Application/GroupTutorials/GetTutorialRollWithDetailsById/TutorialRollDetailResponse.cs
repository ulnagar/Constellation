namespace Constellation.Application.GroupTutorials.GetTutorialRollWithDetailsById;

using Constellation.Core.Enums;
using System;
using System.Collections.Generic;

public sealed record TutorialRollDetailResponse(
    Guid Id,
    DateOnly SessionDate,
    string StaffId,
    TutorialRollStatus Status,
    IReadOnlyCollection<TutorialRollStudentReponse> Students);
