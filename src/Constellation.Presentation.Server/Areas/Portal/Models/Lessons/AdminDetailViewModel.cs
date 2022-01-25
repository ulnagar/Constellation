using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    public class AdminDetailViewModel
    {
        public AdminDetailViewModel()
        {
            Offerings = new List<string>();
            Rolls = new List<RollDto>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        [Display(Name=DisplayNameDefaults.CourseName)]
        public string CourseName { get; set; }
        public ICollection<string> Offerings { get; set; }
        [Display(Name=DisplayNameDefaults.DueDate)]
        public DateTime DueDate { get; set; }
        public bool Overdue { get; set; }
        public ICollection<RollDto> Rolls { get; set; }

        public static AdminDetailViewModel ConvertFromLesson(Lesson lesson)
        {
            var viewModel = new AdminDetailViewModel
            {
                Id = lesson.Id,
                Name = lesson.Name,
                CourseName = lesson.Offerings.First().Course.Name,
                Offerings = lesson.Offerings.Select(offering => offering.Name).ToList(),
                DueDate = lesson.DueDate,
                Overdue = lesson.DueDate < DateTime.Today
            };

            foreach (var roll in lesson.Rolls)
                viewModel.Rolls.Add(RollDto.ConvertFromRoll(roll));

            return viewModel;
        }

        public class RollDto
        {
            public RollDto()
            {
                Attendance = new List<StudentAttendanceDto>();
            }

            public Guid Id { get; set; }
            public bool Submitted { get; set; }
            public DateTime? LessonDate { get; set; }
            public string Comment { get; set; }
            public string CoordinatorName { get; set; }
            public string SchoolName { get; set; }
            public LessonStatus Status { get; set; }
            public ICollection<StudentAttendanceDto> Attendance { get; set; }

            public static RollDto ConvertFromRoll(LessonRoll roll)
            {
                var viewModel = new RollDto
                {
                    Id = roll.Id,
                    Submitted = roll.SubmittedDate.HasValue,
                    Comment = roll.Comment,
                    SchoolName = roll.School.Name,
                    Status = roll.Status
                };

                if (viewModel.Submitted)
                {
                    if (roll.LessonDate.HasValue && roll.SchoolContactId.HasValue)
                    {
                        viewModel.CoordinatorName = roll.SchoolContact.DisplayName;
                    }

                    if (roll.LessonDate.HasValue && !roll.SchoolContactId.HasValue)
                    {
                        viewModel.CoordinatorName = "Submitted by Admin";
                    }

                    viewModel.LessonDate = roll.LessonDate;
                }

                foreach (var attendance in roll.Attendance)
                    viewModel.Attendance.Add(StudentAttendanceDto.ConvertFromAttendance(attendance));

                return viewModel;
            }
        }

        public class StudentAttendanceDto
        {
            public Guid Id { get; set; }
            public string LastName { get; set; }
            public string DisplayName { get; set; }
            public bool Present { get; set; }

            public static StudentAttendanceDto ConvertFromAttendance(LessonRoll.LessonRollStudentAttendance attendance)
            {
                var viewModel = new StudentAttendanceDto
                {
                    Id = attendance.Id,
                    LastName = attendance.Student.LastName,
                    DisplayName = attendance.Student.DisplayName,
                    Present = attendance.Present
                };

                return viewModel;
            }
        }
    }
}