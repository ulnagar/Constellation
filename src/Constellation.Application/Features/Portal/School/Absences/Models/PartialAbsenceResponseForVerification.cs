using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Helpers;
using Constellation.Core.Models.Absences;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.Features.Portal.School.Absences.Models
{
    public class PartialAbsenceResponseForVerification : IMapFrom<Response>
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
    }
}
