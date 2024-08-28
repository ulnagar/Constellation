namespace Constellation.Core.Models.Attachments;

using Errors;
using Identifiers;
using Shared;
using System;
using ValueObjects;

public sealed class Attachment
{
    private Attachment() { } // Required for EFCore

    private Attachment(
        string name,
        string fileType,
        AttachmentType recordType,
        string recordLinkId,
        DateTime createdAt)
    {
        Name = name;
        FileType = fileType;
        LinkType = recordType;
        LinkId = recordLinkId;
        CreatedAt = createdAt;
    }

    public AttachmentId Id { get; private set; } = new();
    public string Name { get; private set; } = string.Empty;
    public string FileType { get; private set; } = string.Empty;
    public byte[] FileData { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public int FileSize { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public AttachmentType LinkType { get; private set; } = AttachmentType.Unset;
    public string LinkId { get; private set; } = string.Empty;
    public string Checksum { get; private set; } = string.Empty;

    public static Attachment CreateAwardCertificateAttachment(
        string name,
        string fileType,
        string recordLinkId,
        DateTime createdAt)
    {
        Attachment attachment = new(
            name,
            fileType,
            AttachmentType.AwardCertificate,
            recordLinkId,
            createdAt);

        return attachment;
    }

    public static Attachment CreateTrainingCertificateAttachment(
        string name,
        string fileType,
        string recordLinkId,
        DateTime createdAt)
    {
        Attachment attachment = new(
            name,
            fileType,
            AttachmentType.TrainingCertificate,
            recordLinkId,
            createdAt);

        return attachment;
    }

    public static Attachment CreateAssignmentSubmissionAttachment(
        string name,
        string fileType,
        string recordLinkId,
        DateTime createdAt)
    {
        Attachment attachment = new(
            name,
            fileType,
            AttachmentType.CanvasAssignmentSubmission,
            recordLinkId,
            createdAt);

        return attachment;
    }

    public static Attachment CreateStudentReportAttachment(
        string name,
        string fileType,
        string recordLinkId,
        DateTime createdAt)
    {
        Attachment attachment = new(
            name,
            fileType,
            AttachmentType.StudentReport,
            recordLinkId,
            createdAt);

        return attachment;
    }

    public static Attachment CreateWorkFlowEmailAttachment(
        string name,
        string fileType,
        string recordLinkId,
        DateTime createdAt)
    {
        Attachment attachment = new(
            name,
            fileType,
            AttachmentType.WorkFlowEmailAttachment,
            recordLinkId,
            createdAt);

        return attachment;
    }

    public static Attachment CreateStudentPhotoAttachment(
        string name,
        string fileType,
        string recordLinkId,
        DateTime createdAt)
    {
        Attachment attachment = new(
            name,
            fileType,
            AttachmentType.StudentPhoto,
            recordLinkId,
            createdAt);

        return attachment;
    }

    public Result AttachData(byte[] fileData, string checksum, bool overwrite = false)
    {
        if (!string.IsNullOrWhiteSpace(FilePath) && overwrite is false)
        {
            return Result.Failure(AttachmentErrors.FilePathExists);
        }

        if (FileData is not null && FileData.Length is not 0 && overwrite is false)
        {
            return Result.Failure(AttachmentErrors.FileDataExists);
        }

        FileData = fileData;
        FileSize = fileData.Length;
        FilePath = string.Empty;
        Checksum = checksum;

        return Result.Success();
    }

    public Result AttachPath(string filePath, int fileSize, string checksum, bool overwrite = false)
    {
        if (FileData is not null && FileData.Length is not 0 && overwrite is false)
        {
            return Result.Failure(AttachmentErrors.FileDataExists);
        }

        if (FilePath?.Length is not 0 && overwrite is false)
        {
            return Result.Failure(AttachmentErrors.FilePathExists);
        }

        FilePath = filePath;
        FileSize = fileSize;
        FileData = null;
        Checksum = checksum;

        return Result.Success();
    }

    public void UpdateChecksum(string checksum) =>
        Checksum = checksum;
}