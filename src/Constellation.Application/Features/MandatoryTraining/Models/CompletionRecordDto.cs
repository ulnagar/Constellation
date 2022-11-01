using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Helpers;
using Constellation.Core.Models.MandatoryTraining;
using System;

namespace Constellation.Application.Features.MandatoryTraining.Models;

public class CompletionRecordDto : IMapFrom<TrainingCompletion>
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public string ModuleName { get; set; }
    public string ModuleExpiry { get; set; }
    public string StaffId { get; set; }
    public string StaffFirstName { get; set; }
    public string StaffLastName { get; set; }
    public string StaffFaculty { get; set; }
    public DateTime CompletedDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<TrainingCompletion, CompletionRecordDto>()
            .ForMember(dest => dest.ModuleExpiry, opt => opt.MapFrom(src => src.Module.Expiry.GetDisplayName()));
    }
}
