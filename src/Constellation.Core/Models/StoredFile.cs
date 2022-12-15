namespace Constellation.Core.Models;

public class StoredFile
{
    public const string CanvasAssignmentSubmission = "Canvas Assignment Submission";
    public const string StudentReport = "Student Report";
    public const string TrainingCertificate = "Training Certificate";

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public DateTime? CreatedAt { get; set; }
    public string LinkType { get; set; } = string.Empty;
    public string LinkId { get; set; } = string.Empty;
}
