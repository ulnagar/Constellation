using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Portal.Pages.Lessons
{
    [Authorize]
    public class TeacherModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public TeacherModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            AllLessons = new List<LessonDto>();
            FilteredLessons = new List<LessonDto>();
        }

        [BindProperty(SupportsGet = true)]
        public FilterDto Filter { get; set; }
        public ICollection<LessonDto> AllLessons { get; set; }
        public ICollection<LessonDto> FilteredLessons { get; set; }
        public SchoolContact Teacher { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var username = User.Identity.Name;
            var coordinator = await _unitOfWork.SchoolContacts.FromEmailForExistCheck(username);

            if (coordinator == null)
                return RedirectToPage("/Error");
            
            Teacher = coordinator;

            var schools = coordinator.Assignments.Where(role => !role.IsDeleted && role.Role == SchoolContactRole.SciencePrac).Select(role => role.School).ToList();

            foreach (var school in schools)
            {
                var lessons = await _unitOfWork.Lessons.GetWithAllRollsForSchool(school.Code);
                foreach (var lesson in lessons)
                {
                    var course = lesson.Offerings.First().Course;
                    var rolls = lesson.Rolls.Where(roll => roll.SchoolCode == school.Code).ToList();

                    foreach (var roll in rolls)
                    {
                        var lessonDto = LessonDto.ConvertFromLesson(lesson);
                        lessonDto.Roll = RollDto.ConvertFromRoll(roll);
                        AllLessons.Add(lessonDto);
                    }
                }
            }

            switch (Filter)
            {
                case FilterDto.All:
                    FilterAll();
                    break;
                case FilterDto.Overdue:
                    FilterOverdue();
                    break;
                case FilterDto.Complete:
                    FilterComplete();
                    break;
                case FilterDto.Pending:
                default:
                    FilterPending();
                    break;
            }

            return Page();
        }

        public void FilterComplete()
        {
            FilteredLessons = AllLessons.Where(lesson => lesson.Roll.Submitted).ToList();
        }

        public void FilterOverdue()
        {
            FilteredLessons = AllLessons.Where(lesson => lesson.Overdue && !lesson.Roll.Submitted).ToList();
        }

        public void FilterAll()
        {
            FilteredLessons = AllLessons;
        }

        public void FilterPending()
        {
            FilteredLessons = AllLessons.Where(lesson => !lesson.Roll.Submitted).ToList();
        }

        public class LessonDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime DueDate { get; set; }
            public bool Overdue { get; set; }
            public string CourseName { get; set; }
            public RollDto Roll { get; set; }

            public static LessonDto ConvertFromLesson(Lesson lesson)
            {
                var viewModel = new LessonDto
                {
                    Id = lesson.Id,
                    Name = lesson.Name,
                    DueDate = lesson.DueDate,
                    Overdue = lesson.DueDate < DateTime.Today,
                    CourseName = $"{lesson.Offerings.First().Course.Grade} {lesson.Offerings.First().Course.Name}"
                };

                return viewModel;
            }
        }

        public class RollDto
        {
            public Guid Id { get; set; }
            public string SchoolCode { get; set; }
            public string SchoolName { get; set; }
            public DateTime? LessonDate { get; set; }
            public bool Submitted { get; set; }
            public string AttendanceStatistics { get; set; }

            public static RollDto ConvertFromRoll(LessonRoll roll)
            {
                var viewModel = new RollDto
                {
                    Id = roll.Id,
                    SchoolCode = roll.SchoolCode,
                    SchoolName = roll.School.Name,
                    LessonDate = roll.LessonDate,
                    Submitted = roll.LessonDate.HasValue,
                    AttendanceStatistics = $"{roll.Attendance.Count(attend => attend.Present)}/{roll.Attendance.Count}"
                };

                return viewModel;
            }
        }

        public enum FilterDto
        {
            Pending,
            All,
            Overdue,
            Complete
        }
    }
}
