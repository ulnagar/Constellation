using Constellation.Application.Helpers;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class LessonRollDto
    {
        public LessonRollDto()
        {
            Attendance = new List<StudentRollDto>();
        }

        public Guid LessonId { get; set; }
        public Guid RollId { get; set; }
        [Display(Name=DisplayNameDefaults.SubmittedDate)]
        public DateTime? SubmittedDate { get; set; }
        public bool Submitted { get; set; }
        public LessonStatus Status { get; set; }
        [Display(Name=DisplayNameDefaults.LessonName)]
        public string LessonName { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name=DisplayNameDefaults.DueDate)]
        public DateTime DueDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name=DisplayNameDefaults.SubmittedDate)]
        public DateTime LessonDate { get; set; }
        public string Comment { get; set; }
        public IList<StudentRollDto> Attendance { get; set; }
        [Display(Name=DisplayNameDefaults.TeacherName)]
        public string TeacherName { get; set; }
        [Display(Name =DisplayNameDefaults.School)]
        public string School { get; set; }

        public static LessonRollDto ConvertFromRoll(LessonRoll roll)
        {
            var viewModel = new LessonRollDto
            {
                LessonId = roll.LessonId,
                RollId = roll.Id,
                SubmittedDate = roll.SubmittedDate,
                Submitted = roll.SubmittedDate.HasValue,
                Status = roll.Status,
                LessonName = roll.Lesson.Name,
                DueDate = roll.Lesson.DueDate,
                LessonDate = roll.LessonDate.HasValue ? roll.LessonDate.Value : DateTime.Today,
                Comment = roll.Comment,
                School = roll.School.Name
            };

            if (roll.LessonDate.HasValue && roll.SchoolContactId.HasValue)
            {
                viewModel.TeacherName = roll.SchoolContact.DisplayName;
            }

            if (roll.LessonDate.HasValue && !roll.SchoolContactId.HasValue)
            {
                viewModel.TeacherName = "Submitted by Admin";
            }

            return viewModel;
        }

        public class StudentRollDto
        {
            public Guid Id { get; set; }
            [Display(Name=DisplayNameDefaults.LastName)]
            public string LastName { get; set; }
            [Display(Name=DisplayNameDefaults.DisplayName)]
            public string DisplayName { get; set; }
            public bool Present { get; set; }

            public static StudentRollDto ConvertFromRollAttendance(LessonRoll.LessonRollStudentAttendance rollAttendance)
            {
                var viewModel = new StudentRollDto
                {
                    Id = rollAttendance.Id,
                    LastName = rollAttendance.Student.LastName,
                    DisplayName = rollAttendance.Student.DisplayName,
                    Present = rollAttendance.Present
                };

                return viewModel;
            }
        }
    }
}
