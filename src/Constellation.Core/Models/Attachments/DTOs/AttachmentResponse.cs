namespace Constellation.Application.Attachments.GetAttachmentFile;

public sealed record AttachmentResponse(
    string FileType,
    string FileName,
    byte[] FileData);