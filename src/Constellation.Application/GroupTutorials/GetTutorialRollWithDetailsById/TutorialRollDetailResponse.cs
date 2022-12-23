namespace Constellation.Application.GroupTutorials.GetTutorialRollWithDetailsById;

using Constellation.Core.Enums;
using System;
using System.Collections.Generic;

public sealed record TutorialRollDetailResponse(
    Guid Id,
    Guid TutorialId,
    string TutorialName,
    DateOnly SessionDate,
    string StaffId,
    string StaffName,
    TutorialRollStatus Status,
    IReadOnlyCollection<TutorialRollStudentResponse> Students);
