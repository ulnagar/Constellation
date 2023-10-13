namespace Constellation.Core.Models.Attachments;

using System;
using ValueObjects;

public sealed class Attachment
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FileType { get; set; }
    public byte[] FileData { get; set; }
    public string FilePath { get; set; }
    public DateTime? CreatedAt { get; set; }
    public AttachmentType LinkType { get; set; }
    public string LinkId { get; set; }
}