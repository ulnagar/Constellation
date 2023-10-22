namespace Constellation.Core.Models.Attachments.DTOs;

public sealed record AttachmentResponse(
    string FileType,
    string FileName,
    byte[] FileData);