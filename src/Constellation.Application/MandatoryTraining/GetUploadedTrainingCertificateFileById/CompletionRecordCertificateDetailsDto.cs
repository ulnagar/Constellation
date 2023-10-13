namespace Constellation.Application.MandatoryTraining.GetUploadedTrainingCertificateFileById;

using AutoMapper;
using Constellation.Application.Common.Mapping;
using Core.Models.Attachments;
using System;

public class CompletionRecordCertificateDetailsDto : IMapFrom<Attachment>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FileType { get; set; }
    public byte[] FileData { get; set; }
    public string FileDataBase64 { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Attachment, CompletionRecordCertificateDetailsDto>()
            .ForMember(dest => dest.FileDataBase64, opt => opt.MapFrom(src => Convert.ToBase64String(src.FileData)));
    }
}
