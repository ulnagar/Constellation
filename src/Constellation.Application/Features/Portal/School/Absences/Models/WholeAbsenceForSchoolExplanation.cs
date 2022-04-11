using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Helpers.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.Features.Portal.School.Absences.Models
{
    public class WholeAbsenceForSchoolExplanation : IMapFrom<Absence>
    {
        [Display(Name = DisplayNameDefaults.Student)]
        public string StudentName { get; set; }
        [Display(Name = DisplayNameDefaults.ClassName)]
        public string ClassName { get; set; }
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        [Display(Name = DisplayNameDefaults.PeriodName)]
        public string PeriodName { get; set; }
        [Display(Name = DisplayNameDefaults.AbsenceTimeframe)]
        public string AbsenceTimeframe { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Absence, WholeAbsenceForSchoolExplanation>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => $"{src.Student.FirstName} {src.Student.LastName}"))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Offering.Name))
                .ForMember(dest => dest.AbsenceTimeframe, opt => opt.MapFrom(src => src.AbsenceTimeframe));
        }
    }
}
