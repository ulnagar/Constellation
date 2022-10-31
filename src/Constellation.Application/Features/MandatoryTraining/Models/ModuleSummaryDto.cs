﻿using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Models.MandatoryTraining;
using System;

namespace Constellation.Application.Features.MandatoryTraining.Models;

public class ModuleSummaryDto : IMapFrom<TrainingModule>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public string Expiry { get; set; }
    public string Url { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<TrainingModule, ModuleSummaryDto>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.DeletedBy)));
    }
}