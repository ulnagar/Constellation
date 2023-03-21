namespace Constellation.Application.GroupTutorials.GetTutorialRollWithDetailsById;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record TutorialRollDetailResponse(
    TutorialRollId Id,
    GroupTutorialId TutorialId,
    string TutorialName,
    DateOnly SessionDate,
    string StaffId,
    string StaffName,
    TutorialRollStatus Status,
    IReadOnlyCollection<TutorialRollStudentResponse> Students);
