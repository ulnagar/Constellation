using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Helpers;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Families;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Constellation.Application.DTOs
{
    public class StudentCompleteDetailsDto : IMapFrom<Student>
    {
        public StudentDetails Student { get; set; }
        public Family Family { get; set; }
        public List<Offering> Enrolments { get; set; }
        public List<Session> Sessions { get; set; }
        public List<Allocation> Equipment { get; set; }
        public List<Absence> Absences { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Student, StudentCompleteDetailsDto>()
                .ForMember(dest => dest.Student, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Family, opt => opt.MapFrom(src => src.Family))
                .ForMember(dest => dest.Enrolments, opt => opt.MapFrom(src => src.Enrolments.Select(enrolment => enrolment.Offering)))
                .ForMember(dest => dest.Sessions, opt => opt.MapFrom(src => src.Enrolments.SelectMany(enrolment => enrolment.Offering.Sessions)))
                .ForMember(dest => dest.Equipment, opt => opt.MapFrom(src => src.Devices))
                .ForMember(dest => dest.Absences, opt => opt.MapFrom(src => src.Absences));
        }

        public class StudentDetails : IMapFrom<Student>
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
        }

        public class Offering : IMapFrom<CourseOffering>
        {
            public int OfferingId { get; set; }
            public string Name { get; set; }
            [Display(Name = DisplayNameDefaults.CourseName)]
            public string CourseName { get; set; }
            public IList<string> Teachers { get; set; }

            public void Mapping(Profile profile)
            {
                profile.CreateMap<CourseOffering, Offering>()
                    .ForMember(dest => dest.OfferingId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.Teachers, opt => opt.MapFrom(src => src.Sessions.Where(session => !session.IsDeleted).Select(session => session.Teacher.DisplayName).Distinct()));
            }
        }

        public class Session : IMapFrom<OfferingSession>
        {
            public TimetablePeriod Period { get; set; }
            [Display(Name = DisplayNameDefaults.ClassName)]
            public string OfferingName { get; set; }
            [Display(Name = DisplayNameDefaults.TeacherName)]
            public string TeacherName { get; set; }
            [Display(Name = DisplayNameDefaults.RoomName)]
            public string RoomName { get; set; }
            public int Duration { get; set; }

            public void Mapping(Profile profile)
            {
                profile.CreateMap<OfferingSession, Session>()
                    .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher.DisplayName))
                    .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => Convert.ToInt32(src.Period.EndTime.Subtract(src.Period.StartTime).TotalMinutes)));
            }
        }

        public class Allocation : IMapFrom<DeviceAllocation>
        {
            [Display(Name = DisplayNameDefaults.DeviceMake)]
            public string DeviceMake { get; set; }
            [Display(Name = DisplayNameDefaults.DeviceSerial)]
            public string DeviceSerial { get; set; }
            [Display(Name = DisplayNameDefaults.DeviceStatus)]
            public string DeviceStatus { get; set; }
            public DateTime Allocated { get; set; }
            public DateTime? Deleted { get; set; }

            public void Mapping(Profile profile)
            {
                profile.CreateMap<DeviceAllocation, Allocation>()
                    .ForMember(dest => dest.DeviceSerial, opt => opt.MapFrom(src => src.Device.SerialNumber))
                    .ForMember(dest => dest.Allocated, opt => opt.MapFrom(src => src.DateAllocated))
                    .ForMember(dest => dest.Deleted, opt => opt.MapFrom(src => src.DateDeleted));
            }
        }

        public class Absence : IMapFrom<Constellation.Core.Models.Absence>
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

            public void Mapping(Profile profile)
            {
                profile.CreateMap<Constellation.Core.Models.Absence, Absence>()
                    .ForMember(dest => dest.Timeframe, opt => opt.MapFrom(src => src.AbsenceTimeframe))
                    .ForMember(dest => dest.NotificationCount, opt => opt.MapFrom(src => src.Notifications.Count))
                    .ForMember(dest => dest.ResponsesCount, opt => opt.MapFrom(src => src.Responses.Count))
                    .ForMember(dest => dest.Explained, opt => opt.MapFrom(src => src.Explained || src.ExternallyExplained));
            }
        }
    }
}
