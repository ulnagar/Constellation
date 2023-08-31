using Constellation.Core.Models.Offerings.Identifiers;
using System;
using System.Collections.Generic;

namespace Constellation.Application.DTOs
{
    public class ClassMonitorDtos
    {
        public ClassMonitorDtos()
        {
            Courses = new List<MonitorCourse>();
            Periods = new List<MonitorPeriod>();
            Users = new List<MonitorUser>();
        }

        public ICollection<MonitorCourse> Courses { get; set; }
        public ICollection<MonitorPeriod> Periods { get; set; }
        public ICollection<MonitorUser> Users { get; set; }
        public DateTime Refreshed { get; set; }

        public class MonitorCourse
        {
            public MonitorCourse()
            {
                Enrolments = new List<MonitorCourseEnrolment>();
                Sessions = new List<MonitorCourseSession>();
                Teachers = new List<MonitorCourseTeacher>();
                Covers = new List<MonitorCourseCover>();

                OtherAttendees = new List<MonitorCourseEnrolment>();
                OtherStaff = new List<MonitorCourseTeacher>();
            }

            public const string Current = " status-current";
            public const string Covered = " status-covered";
            public const string StudentsPresent = " status-students";
            public const string TeachersPresent = " status-teachers";

            public OfferingId Id { get; set; }
            public string Name { get; set; }
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            public bool IsCurrent { get; set; }
            public string GradeShortCode { get; set; }
            public string GradeName { get; set; }
            public string RoomScoId { get; set; }
            public string RoomName { get; set; }
            public string RoomUrlPath { get; set; }

            public ICollection<MonitorCourseEnrolment> Enrolments { get; set; }
            public ICollection<MonitorCourseSession> Sessions { get; set; }
            public ICollection<MonitorCourseTeacher> Teachers { get; set; }
            public ICollection<MonitorCourseCover> Covers { get; set; }

            public string StatusCode { get; set; }
            public int Guests { get; set; }
            public ICollection<MonitorCourseEnrolment> OtherAttendees { get; set; }
            public ICollection<MonitorCourseTeacher> OtherStaff { get; set; }

            public DateTime LastScanTime { get; set; }
        }

        public class MonitorCourseEnrolment
        {
            public int Id { get; set; }
            public string StudentId { get; set; }
            public string StudentDisplayName { get; set; }
            public string StudentLastName { get; set; }
            public string StudentGender { get; set; }
            public bool IsDeleted { get; set; }
            public bool IsPresent { get; set; }
        }

        public class MonitorCourseSession
        {
            public int Id { get; set; }
            public int PeriodId { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class MonitorCourseTeacher
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string LastName { get; set; }
            public bool IsDeleted { get; set; }
            public bool IsPresent { get; set; }
        }

        public class MonitorCourseCover
        {
            public Guid Id { get; set; }
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            public string PersonId { get; set; }
            public string PersonName { get; set; }
            public bool IsCurrent { get; set; } //=> (StartDate <= _dtHelper.Today() && _dtHelper.Today() <= EndDate);
            public bool IsPresent { get; set; }
        }

        public class MonitorPeriod
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Timetable { get; set; }
            public string Type { get; set; }
            public int Day { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }

            public bool
                IsCurrent
            {
                get;
                set;
            } //(Day == TimeProvider.Current.DayNumber && StartTime <= TimeProvider.Current.Now.TimeOfDay && EndTime >= TimeProvider.Current.Now.TimeOfDay);

            public string DisplayName { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class MonitorUser
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string Gender { get; set; }
            public string UserType { get; set; }
            public string UserPrincipalId { get; set; }
            public bool IsDeleted { get; set; }
        }
    }
}