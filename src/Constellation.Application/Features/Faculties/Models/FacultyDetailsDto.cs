namespace Constellation.Application.Features.Faculties.Models;

using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class FacultyDetailsDto : IMapFrom<Faculty>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Colour { get; set; }
    public List<FacultyMemberSummaryDto> Members { get; set; } = new();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Faculty, FacultyDetailsDto>()
            .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members.Where(member => !member.IsDeleted)));
    }
}

public class FacultyMemberSummaryDto : IMapFrom<FacultyMembership>
{
    public Guid Id { get; set; }
    public string StaffId { get; set; }
    public string StaffFirstName { get; set; }
    public string StaffLastName { get; set; }
    public string Role { get; set; }
}
