using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Student_DetailsViewModel : BaseViewModel
    {
        public StudentDto Student { get; set; }
        public StudentFamily Family { get; set; }
        public List<OfferingDto> Enrolments { get; set; }
        public List<SessionDto> Sessions { get; set; }
        public List<AllocationDto> Equipment { get; set; }
        public List<AbsenceDto> Absences { get; set; }

        // View Properties
        public int MinPerFn { get; set; }
        public SelectList OfferingList { get; set; }

        public class StudentDto
        {
            [Display(Name = DisplayNameDefaults.StudentId)]
            public string StudentId { get; set; }
            [Display(Name = DisplayNameDefaults.IsDeleted)]
            public bool IsDeleted { get; set; }
            [Display(Name = DisplayNameDefaults.DisplayName)]
            public string DisplayName { get; set; }
            [Display(Name = DisplayNameDefaults.DateEntered)]
            public DateTime? DateEntered { get; set; }
            [Display(Name = DisplayNameDefaults.EmailAddress)]
            public string EmailAddress { get; set; }
            [Display(Name = DisplayNameDefaults.DateDeleted)]
            public DateTime? DateDeleted { get; set; }
            [Display(Name = DisplayNameDefaults.SchoolName)]
            public string SchoolName { get; set; }
            [Display(Name = DisplayNameDefaults.SchoolCode)]
            public string SchoolCode { get; set; }
            [Display(Name = DisplayNameDefaults.AdobeConnectId)]
            public string AdobeConnectPrincipalId { get; set; }
            [Display(Name = DisplayNameDefaults.CurrentGrade)]
            public Grade CurrentGrade { get; set; }

            public static StudentDto ConvertFromStudent(Student student)
            {
                var viewModel = new StudentDto
                {
                    StudentId = student.StudentId,
                    IsDeleted = student.IsDeleted,
                    DisplayName = student.DisplayName,
                    DateEntered = student.DateEntered,
                    EmailAddress = student.EmailAddress,
                    DateDeleted = student.DateDeleted,
                    SchoolName = student.School.Name,
                    SchoolCode = student.SchoolCode,
                    AdobeConnectPrincipalId = student.AdobeConnectPrincipalId,
                    CurrentGrade = student.CurrentGrade
                };

                return viewModel;
            }
        }

        public class OfferingDto
            {
                public int OfferingId { get; set; }
                public string Name { get; set; }
                [Display(Name = DisplayNameDefaults.CourseName)]
                public string CourseName { get; set; }
                public IList<string> Teachers { get; set; }

                public static OfferingDto ConvertFromOffering(CourseOffering offering)
                {
                    var viewModel = new OfferingDto
                    {
                        OfferingId = offering.Id,
                        Name = offering.Name,
                        CourseName = offering.Course.Name,
                        Teachers = offering.Sessions.Where(s => !s.IsDeleted).Select(s => s.Teacher.DisplayName).Distinct().ToList()
                    };

                    return viewModel;
                }
            }

        public class SessionDto
        {
            public TimetablePeriod Period { get; set; }
            [Display(Name = DisplayNameDefaults.ClassName)]
            public string OfferingName { get; set; }
            [Display(Name = DisplayNameDefaults.TeacherName)]
            public string TeacherName { get; set; }
            [Display(Name = DisplayNameDefaults.RoomName)]
            public string RoomName { get; set; }
            public int Duration { get; set; }

            public static SessionDto ConvertFromSession(OfferingSession session)
            {
                var viewModel = new SessionDto
                {
                    Period = session.Period,
                    OfferingName = session.Offering.Name,
                    TeacherName = session.Teacher.DisplayName,
                    RoomName = session.Room.Name
                };

                viewModel.Duration = session.Period.EndTime.Subtract(session.Period.StartTime).Minutes;
                viewModel.Duration += session.Period.EndTime.Subtract(session.Period.StartTime).Hours * 60;

                return viewModel;
            }
        }

        public class AllocationDto
        {
            [Display(Name = DisplayNameDefaults.DeviceMake)]
            public string DeviceMake { get; set; }
            [Display(Name = DisplayNameDefaults.DeviceSerial)]
            public string DeviceSerial { get; set; }
            [Display(Name = DisplayNameDefaults.DeviceStatus)]
            public string DeviceStatus { get; set; }
            public DateTime Allocated { get; set; }
            public DateTime? Deleted { get; set; }

            public static AllocationDto ConvertFromDeviceAllocation(DeviceAllocation device)
            {
                var viewModel = new AllocationDto
                {
                    DeviceMake = device.Device.Make,
                    DeviceSerial = device.SerialNumber,
                    DeviceStatus = device.Device.Status.ToString(),
                    Allocated = device.DateAllocated,
                    Deleted = device.DateDeleted
                };

                return viewModel;
            }
        }

        public class AbsenceDto
        {
            public Guid Id { get; set; }
            public string Type { get; set; }
            public DateTime Date { get; set; }
            public string Timeframe { get; set; }
            [Display(Name = DisplayNameDefaults.PeriodTimeframe)]
            public string PeriodTimeframe { get; set; }
            [Display(Name = DisplayNameDefaults.PeriodName)]
            public string PeriodName { get; set; }
            [Display(Name = DisplayNameDefaults.ClassName)]
            public string OfferingName { get; set; }
            public int NotificationCount { get; set; }
            public int ResponsesCount { get; set; }
            public bool Explained { get; set; }

            public static AbsenceDto ConvertFromAbsence(Absence absence)
            {
                var viewModel = new AbsenceDto
                {
                    Id = absence.Id,
                    Type = absence.Type,
                    Date = absence.Date,
                    Timeframe = absence.AbsenceTimeframe,
                    PeriodTimeframe = absence.PeriodTimeframe,
                    PeriodName = absence.PeriodName,
                    OfferingName = absence.Offering.Name,
                    NotificationCount = absence.Notifications.Count,
                    ResponsesCount = absence.Responses.Count,
                    Explained = absence.Explained || absence.ExternallyExplained
                };

                return viewModel;
            }
        }
    }
}