namespace Constellation.Application.SciencePracs.GetLessonRollSubmitContextForSchoolsPortal;

using Constellation.Application.Helpers;
using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ScienceLessonRollForSubmit
{
    public SciencePracRollId Id { get; set; }

    public SciencePracLessonId LessonId { get; set; }

    [Display(Name = DisplayNameDefaults.LessonName)]
    public string LessonName { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = DisplayNameDefaults.DueDate)]
    public DateTime LessonDueDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = DisplayNameDefaults.SubmittedDate)]
    public DateTime LessonDate { get; set; } = DateTime.Today;

    public string Comment { get; set; } = string.Empty;

    public List<StudentAttendance> Attendance { get; set; } = new();

    [Display(Name = DisplayNameDefaults.TeacherName)]
    public string TeacherName { get; set; }

    public class StudentAttendance
    {
        public SciencePracAttendanceId Id { get; set; }

        public string StudentId { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string StudentName => $"{StudentFirstName} {StudentLastName}";

        public bool Present { get; set; }
    }
}
