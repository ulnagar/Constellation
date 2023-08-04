using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Enums;
using Constellation.Core.Models.SciencePracs;
using System;
using System.Linq;

namespace Constellation.Application.Features.Portal.School.ScienceRolls.Models
{
    public class ScienceLessonRollForList : IMapFrom<SciencePracRoll>
    {
        public Guid Id { get; set; }
        public Guid LessonId { get; set; }
        public string LessonName { get; set; }
        public Grade LessonGrade { get; set; }
        public string Grade => $"Year {((int)LessonGrade):D2}";
        public string LessonCourseName { get; set; }
        public DateTime LessonDueDate { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsOverdue => LessonDueDate < DateTime.Now && !IsSubmitted;
        public string Statistics { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<SciencePracRoll, ScienceLessonRollForList>()
                .ForMember(dest => dest.LessonGrade, opt => opt.MapFrom(src => src.Lesson.Offerings.First().Course.Grade))
                .ForMember(dest => dest.IsSubmitted, opt => opt.MapFrom(src => src.LessonDate.HasValue))
                .ForMember(dest => dest.LessonCourseName, opt => opt.MapFrom(src => src.Lesson.Offerings.First().Course.Name))
                .ForMember(dest => dest.Statistics, opt => opt.MapFrom(src => $"{src.Attendance.Count(attend => attend.Present)}/{src.Attendance.Count}"));
        }

    }
}
