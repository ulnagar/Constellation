namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonRollsForSchoolsPortal;

using Core.Enums;
using Core.Models.Identifiers;
using System;

public sealed class ScienceLessonRollSummary
{
    public SciencePracRollId Id { get; set; }
    public SciencePracLessonId LessonId { get; set; }
    public string LessonName { get; set; }
    public Grade LessonGrade { get; set; }
    public string Grade => $"Year {(int)LessonGrade:D2}";
    public string LessonCourseName { get; set; }
    public DateTime LessonDueDate { get; set; }
    public bool IsSubmitted { get; set; }
    public bool IsOverdue => LessonDueDate < DateTime.Today && !IsSubmitted;
    public string Statistics { get; set; }
}
