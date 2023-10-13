namespace Constellation.Application.MandatoryTraining.GetUploadedTrainingCertificationMetadata;

using Core.Models.Attachments.Identifiers;

public class CompletionRecordCertificateDto
{
    public AttachmentId Id { get; set; }
    public string Name { get; set; }
    public string FileType { get; set; }
}
