namespace Constellation.Application.Features.Faculties.Models;

using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using System;
using System.Linq;

public class FacultySummaryDto : IMapFrom<Faculty>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Colour { get; set; }
    public int MemberCount { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Faculty, FacultySummaryDto>()
            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Members.Count(member => !member.IsDeleted)));
    }
}
