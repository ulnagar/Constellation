namespace Constellation.Core.Models.Attachments.Enums;

using Common;

public sealed class AttachmentType : StringEnumeration<AttachmentType>
{
    public static readonly AttachmentType Empty = new(string.Empty);

    public static readonly AttachmentType Unset = new("Unset");
    public static readonly AttachmentType AssessmentDownload = new("Assessment Download");
    public static readonly AttachmentType AssessmentSubmission = new("Assessment Submission");
    public static readonly AttachmentType CanvasAssignmentSubmission = new("Canvas Assignment Submission");
    public static readonly AttachmentType StudentReport = new("Student Report");
    public static readonly AttachmentType ExternalReport = new("External Report");
    public static readonly AttachmentType TrainingCertificate = new("Training Certificate");
    public static readonly AttachmentType AwardCertificate = new("Award Certificate");
    public static readonly AttachmentType WorkFlowEmailAttachment = new("WorkFlow Email Attachment");
    public static readonly AttachmentType StudentPhoto = new("Student Photo");
    public static readonly AttachmentType TempFile = new("Temporary File");

    private AttachmentType(string value)
        : base(value, value) { }

    public static IEnumerable<AttachmentType> GetOptions => GetEnumerable;
}