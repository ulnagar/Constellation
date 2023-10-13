namespace Constellation.Application.MandatoryTraining.GetUploadedTrainingCertificateFileById;

using Core.Models.Attachments.Identifiers;

public class CompletionRecordCertificateDetailsDto
{
    public AttachmentId Id { get; set; }
    public string Name { get; set; }
    public string FileType { get; set; }
    public byte[] FileData { get; set; }
    public string FileDataBase64 { get; set; }
}
