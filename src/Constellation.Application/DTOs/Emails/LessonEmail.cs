namespace Constellation.Application.DTOs;

using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record LessonEmail(
    string SchoolCode,
    string SchoolName,
    List<LessonEmail.LessonItem> Lessons)
{
    public sealed record LessonItem(
        SciencePracLessonId LessonId,
        SciencePracRollId RollId,
        string Name,
        DateOnly DueDate,
        int NotificationCount);
}
