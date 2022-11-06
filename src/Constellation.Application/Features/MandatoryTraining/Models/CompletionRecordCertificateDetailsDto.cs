namespace Constellation.Application.Features.MandatoryTraining.Models;

using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using System;

public class CompletionRecordCertificateDetailsDto : IMapFrom<StoredFile>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FileType { get; set; }
    public string FileData { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<StoredFile, CompletionRecordCertificateDetailsDto>()
            .ForMember(dest => dest.FileData, opt => opt.MapFrom(src => Convert.ToBase64String(src.FileData)));
    }
}
