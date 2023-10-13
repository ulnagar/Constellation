namespace Constellation.Core.Models.Attachments;

using Errors;
using Identifiers;
using Shared;
using System;
using ValueObjects;

public sealed class Attachment
{
    private Attachment(
        string name,
        string fileType,
        AttachmentType recordType,
        string recordLinkId,
        DateTime createdAt)
    {
        Id = new();
        Name = name;
        FileType = fileType;
        LinkType = recordType;
        LinkId = recordLinkId;
        CreatedAt = createdAt;
    }

    public AttachmentId Id { get; private set; }
    public string Name { get; private set; }
    public string FileType { get; private set; }
    public byte[] FileData { get; private set; }
    public string FilePath { get; private set; }
    public int FileSize { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public AttachmentType LinkType { get; private set; }
    public string LinkId { get; private set; }

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

    public Result AttachData(byte[] fileData, bool overwrite = false)
    {
        if (FilePath is not null)
        {
            return Result.Failure(AttachmentErrors.FilePathExists);
        }

        if (FileData is not null && overwrite is false)
        {
            return Result.Failure(AttachmentErrors.FileDataExists);
        }

        FileData = fileData;
        FileSize = fileData.Length;
        FilePath = null;

        return Result.Success();
    }

    public Result AttachPath(string filePath, int fileSize)
    {
        if (FileData is not null)
        {
            return Result.Failure(AttachmentErrors.FileDataExists);
        }

        if (FilePath is not null)
        {
            return Result.Failure(AttachmentErrors.FilePathExists);
        }

        FilePath = filePath;
        FileSize = fileSize;
        FileData = null;

        return Result.Success();
    }
}