using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Helpers.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.Features.Portal.School.Absences.Models
{
    public class PartialAbsenceResponseForVerification : IMapFrom<AbsenceResponse>
    {
        [Display(Name = DisplayNameDefaults.Student)]
        public string StudentName { get; set; }
        [Display(Name = DisplayNameDefaults.ClassName)]
        public string ClassName { get; set; }
        public Guid AbsenceId { get; set; }
        public DateTime AbsenceDate { get; set; }
        [Display(Name = DisplayNameDefaults.PeriodName)]
        public string AbsencePeriodName { get; set; }
        [Display(Name = DisplayNameDefaults.PeriodTimeframe)]
        public string AbsencePeriodTimeframe { get; set; }
        [Display(Name = DisplayNameDefaults.AbsenceTimeframe)]
        public string AbsenceTimeframe { get; set; }
        [Display(Name = DisplayNameDefaults.AbsenceLength)]
        public int AbsenceLength { get; set; }
        public string Explanation { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<AbsenceResponse, PartialAbsenceResponseForVerification>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => $"{src.Absence.Student.FirstName} {src.Absence.Student.LastName}"))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Absence.Offering.Name))
                .ForMember(dest => dest.AbsenceTimeframe, opt => opt.MapFrom(src => src.Absence.AbsenceTimeframe))
                .ForMember(dest => dest.AbsenceLength, opt => opt.MapFrom(src => src.Absence.AbsenceLength));
        }
    }
}
