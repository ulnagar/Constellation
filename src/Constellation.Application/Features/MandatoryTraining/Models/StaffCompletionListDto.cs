namespace Constellation.Application.Features.MandatoryTraining.Models;

using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using System.Collections.Generic;
using System.Linq;

public class StaffCompletionListDto : IMapFrom<Staff>
{
    public string StaffId { get; set; }
    public string Name { get; set; }
    public List<string> Faculties { get; set; }
    public string SchoolName { get; set; }
    public string EmailAddress { get;set; }
    public List<CompletionRecordExtendedDetailsDto> Modules { get; set; } = new();
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Staff, StaffCompletionListDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DisplayName))
            .ForMember(dest => dest.Faculties, opt => opt.MapFrom(src => src.Faculties.Where(member => !member.IsDeleted).Select(member => member.Faculty.Name).ToList()))
            .ForMember(dest => dest.Modules, opt => opt.Ignore());
    }
}
