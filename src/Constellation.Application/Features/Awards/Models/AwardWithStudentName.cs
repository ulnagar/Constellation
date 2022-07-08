using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Extensions;
using Constellation.Core.Models;
using System;

namespace Constellation.Application.Features.Awards.Models
{
    public class AwardWithStudentName : IMapFrom<StudentAward>
    {
        public Guid Id { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentGrade { get; set; }
        public string StudentSchool { get; set; }
        public string AwardType { get; set; }
        public DateTime AwardedOn { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<StudentAward, AwardWithStudentName>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.DisplayName))
                .ForMember(dest => dest.StudentSchool, opt => opt.MapFrom(src => src.Student.School.Name))
                .ForMember(dest => dest.StudentGrade, opt => opt.MapFrom(src => src.Student.CurrentGrade.AsName()))
                .ForMember(dest => dest.AwardType, opt => opt.MapFrom(src => src.Type));
        }
    }
}
