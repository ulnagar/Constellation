using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Subject.Models
{
    public class Classes_DetailsViewModel : BaseViewModel
    {
        public Classes_DetailsViewModel()
        {
            Students = new List<StudentDto>();
            Sessions = new List<SessionDto>();
            Lessons = new List<LessonDto>();
        }

        public OfferingDto Class { get; set; }
        public ICollection<StudentDto> Students { get; set; }
        public ICollection<SessionDto> Sessions { get; set; }
        public ICollection<LessonDto> Lessons { get; set; }
        public Sessions_AssignmentViewModel AddSessionDto { get; set; }

        public SelectList StudentList { get; set; }

        public class OfferingDto
        {
            public OfferingDto()
            {
                RoomLinks = new List<string>();
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public string CourseName { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsCurrent { get; set; }
            public DateTime StartDate { get; set; }
            public decimal FTECalculation { get; set; }
            public ICollection<string> RoomLinks { get; set; }
            public int MinPerFN { get; set; }

            public static OfferingDto ConvertFromOffering(CourseOffering offering)
            {
                var viewModel = new OfferingDto
                {
                    Id = offering.Id,
                    Name = offering.Name,
                    CourseName = offering.Course.Name,
                    EndDate = offering.EndDate,
                    StartDate = offering.StartDate,
                    FTECalculation = offering.Enrolments.Count(enrolment => !enrolment.IsDeleted) * offering.Course.FullTimeEquivalentValue,
                    RoomLinks = offering.Sessions.Where(session => !session.IsDeleted).Select(session => session.Room.UrlPath).Distinct().ToList(),
                    IsCurrent = offering.StartDate <= DateTime.Now && offering.EndDate >= DateTime.Now
                };

                viewModel.MinPerFN = offering.Sessions.Where(session => !session.IsDeleted).Sum(session => session.Period.EndTime.Subtract(session.Period.StartTime).Minutes);
                viewModel.MinPerFN = offering.Sessions.Where(session => !session.IsDeleted).Sum(session => session.Period.EndTime.Subtract(session.Period.StartTime).Hours * 60);

                return viewModel;
            }
        }

        public class StudentDto
        {
            public string StudentId { get; set; }
            public string Gender { get; set; }
            public string Name { get; set; }
            public Grade Grade { get; set; }
            public string SchoolName { get; set; }

            public static StudentDto ConvertFromStudent(Student student)
            {
                var viewModel = new StudentDto
                {
                    StudentId = student.StudentId,
                    Gender = student.Gender,
                    Name = student.DisplayName,
                    Grade = student.CurrentGrade,
                    SchoolName = student.School.Name
                };

                return viewModel;
            }
        }

        public class SessionDto
        {
            public int Id { get; set; }
            public string Teacher { get; set; }
            public TimetablePeriod Period { get; set; }
            public string RoomName { get; set; }
            public string RoomLink { get; set; }
            public int Duration { get; set; }

            public static SessionDto ConvertFromSession(OfferingSession session)
            {
                var viewModel = new SessionDto
                {
                    Id = session.Id,
                    Teacher = session.Teacher.DisplayName,
                    Period = session.Period,
                    RoomName = session.Room.Name,
                    RoomLink = session.Room.UrlPath
                };

                viewModel.Duration = session.Period.EndTime.Subtract(session.Period.StartTime).Minutes;
                viewModel.Duration += session.Period.EndTime.Subtract(session.Period.StartTime).Hours * 60;

                return viewModel;
            }
        }

        public class LessonDto
        {
            public LessonDto()
            {
                Students = new List<LessonStudentDto>();
            }

            public Guid Id { get; set; }
            public DateTime DueDate { get; set; }
            public string Name { get; set; }
            public ICollection<LessonStudentDto> Students { get; set; }

            public static LessonDto ConvertFromLesson(Lesson lesson, CourseOffering offering)
            {
                var viewModel = new LessonDto
                {
                    Id = lesson.Id,
                    DueDate = lesson.DueDate,
                    Name = lesson.Name
                };

                var enrolledStudents = offering.Enrolments.Where(enrol => !enrol.IsDeleted).Select(enrol => enrol.StudentId).ToList();

                foreach (var student in lesson.Rolls.SelectMany(roll => roll.Attendance).Where(attend => enrolledStudents.Contains(attend.StudentId)))
                {
                    viewModel.Students.Add(LessonStudentDto.ConvertFromLessonAttendance(student));
                }

                return viewModel;
            }
        }

        public class LessonStudentDto
        {
            public string Name { get; set; }
            public string SchoolName { get; set; }
            public LessonStatus Status { get; set; }
            public bool WasPresent { get; set; }
            public string Comment { get; set; }

            public static LessonStudentDto ConvertFromLessonAttendance(LessonRoll.LessonRollStudentAttendance attendance)
            {
                var viewModel = new LessonStudentDto
                {
                    Name = attendance.Student.DisplayName,
                    SchoolName = attendance.Student.School.Name,
                    Status = attendance.LessonRoll.Status,
                    WasPresent = attendance.Present,
                    Comment = attendance.LessonRoll.Comment
                };

                return viewModel;
            }
        }
    }
}