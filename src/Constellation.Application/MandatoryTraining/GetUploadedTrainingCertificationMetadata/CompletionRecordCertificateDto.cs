namespace Constellation.Application.MandatoryTraining.GetUploadedTrainingCertificationMetadata;

using Constellation.Application.Common.Mapping;
using Core.Models.Attachments;

public class CompletionRecordCertificateDto : IMapFrom<Attachment>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FileType { get; set; }
}
