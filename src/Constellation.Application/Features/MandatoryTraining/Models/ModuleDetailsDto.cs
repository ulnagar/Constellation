using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Helpers;
using Constellation.Core.Models.MandatoryTraining;
using System;
using System.Collections.Generic;

namespace Constellation.Application.Features.MandatoryTraining.Models;

public class ModuleDetailsDto : IMapFrom<TrainingModule>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Expiry { get; set; }
    public string Url { get; set; }
    public List<CompletionRecordDto> Completions { get; set; } = new();
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<TrainingModule, ModuleDetailsDto>()
            .ForMember(dest => dest.Expiry, opt => opt.MapFrom(src => src.Expiry.GetDisplayName()))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted));
    }
}
