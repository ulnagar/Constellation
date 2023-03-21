namespace Constellation.Application.MandatoryTraining.GetUploadedTrainingCertificateFileById;

using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using System;

public class CompletionRecordCertificateDetailsDto : IMapFrom<StoredFile>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FileType { get; set; }
    public byte[] FileData { get; set; }
    public string FileDataBase64 { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<StoredFile, CompletionRecordCertificateDetailsDto>()
            .ForMember(dest => dest.FileDataBase64, opt => opt.MapFrom(src => Convert.ToBase64String(src.FileData)));
    }
}
