using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Models.Absences;
using System;
using System.Linq;

namespace Constellation.Application.Features.Portal.School.Absences.Models
{
    public class AbsenceForPortalList : IMapFrom<Absence>
    {
        public Guid Id { get; set; }
        public string StudentName => $"{StudentFirstName} {StudentLastName}";
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string StudentCurrentGrade { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string PeriodName { get; set; }
        public string PeriodTimeframe { get; set; }
        public int AbsenceLength { get; set; }
        public string AbsenceTimeframe { get; set; }
        public string OfferingName { get; set; }
        public Guid? ReasonId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Absence, AbsenceForPortalList>()
                .ForMember(dest => dest.ReasonId, opt => 
                {
                    opt.PreCondition(src => src.Responses.Any(response => response.VerificationStatus == AbsenceResponse.Pending));
                    opt.MapFrom(src => src.Responses.First(response => response.VerificationStatus == AbsenceResponse.Pending).Id);
                });
        }
    }
}
