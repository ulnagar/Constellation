using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    public class AdminViewModel
    {
        public AdminViewModel()
        {
            Lessons = new List<LessonDto>();
        }

        public ICollection<LessonDto> Lessons { get; set; }
        public string filter { get; set; }
        public bool SearchResult { get; set; }

        public AdminSearchViewModel SearchViewModel { get; set; }

        public class LessonDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string CourseName { get; set; }
            public DateTime DueDate { get; set; }
            public bool Overdue { get; set; }
            public string AttendanceStatistics { get; set; }
            public LessonStatus Status { get; set; }

            public static LessonDto ConvertFromLesson(Lesson lesson)
            {
                var viewModel = new LessonDto
                {
                    Id = lesson.Id,
                    Name = lesson.Name,
                    CourseName = $"{lesson.Offerings.First().Course.Grade} {lesson.Offerings.First().Course.Name}",
                    DueDate = lesson.DueDate,
                    Overdue = lesson.DueDate < DateTime.Today,
                    AttendanceStatistics = $"{lesson.Rolls.Count(roll => roll.LessonDate.HasValue)}/{lesson.Rolls.Count(l => l.Status != LessonStatus.Cancelled)}"
                };

                return viewModel;
            }

            public static LessonDto ConvertFromRoll(LessonRoll roll)
            { 
                var viewModel = new LessonDto
                {
                    Id = roll.Id,
                    Name = roll.Lesson.Name,
                    CourseName = $"{roll.Lesson.Offerings.First().Course.Grade} {roll.Lesson.Offerings.First().Course.Name}",
                    DueDate = roll.Lesson.DueDate,
                    Overdue = roll.Lesson.DueDate < DateTime.Today && roll.Status != LessonStatus.Completed && roll.Status != LessonStatus.Cancelled,
                    AttendanceStatistics = $"{roll.Attendance.Count(a => a.Present)}/{roll.Attendance.Count}",
                    Status = roll.Status
                };

                return viewModel;
            }
        }
    }
}