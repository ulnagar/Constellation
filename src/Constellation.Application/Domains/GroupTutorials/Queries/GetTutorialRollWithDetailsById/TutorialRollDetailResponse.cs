namespace Constellation.Application.Domains.GroupTutorials.Queries.GetTutorialRollWithDetailsById;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using System;
using System.Collections.Generic;

public sealed record TutorialRollDetailResponse(
    TutorialRollId Id,
    GroupTutorialId TutorialId,
    string TutorialName,
    DateOnly SessionDate,
    StaffId StaffId,
    string StaffName,
    TutorialRollStatus Status,
    IReadOnlyCollection<TutorialRollStudentResponse> Students);
