namespace Constellation.Application.Training.Modules.GetUploadedTrainingCertificateFileById;

public class CompletionRecordCertificateDetailsDto
{
    public string Name { get; set; }
    public string FileType { get; set; }
    public byte[] FileData { get; set; }
    public string FileDataBase64 { get; set; }
}
