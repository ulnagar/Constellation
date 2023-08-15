namespace Constellation.Application.SciencePracs.GetLessonRollDetailsForSchoolsPortal;

using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public class ScienceLessonRollDetails
{
    public SciencePracRollId Id { get; set; }
    public SciencePracLessonId LessonId { get; set; }
    public string LessonName { get; set; }
    public DateTime LessonDueDate { get; set; }
    public DateTime? LessonDate { get; set; }
    public string SchoolContactFirstName { get; set; }
    public string SchoolContactLastName { get; set; }
    public string SchoolContactName => $"{SchoolContactFirstName} {SchoolContactLastName}";
    public string Comment { get; set; }
    public List<RollAttendance> Attendance { get; set; } = new();

    public class RollAttendance
    {
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string StudentName => $"{StudentFirstName} {StudentLastName}";
        public bool Present { get; set; }
    }
}
