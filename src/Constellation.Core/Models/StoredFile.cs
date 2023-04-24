using System;

namespace Constellation.Core.Models
{
    public class StoredFile
    {
        public const string CanvasAssignmentSubmission = "Canvas Assignment Submission";
        public const string StudentReport = "Student Report";
        public const string TrainingCertificate = "Training Certificate";
        public const string AwardCertificate = "Award Certificate";

        public int Id { get; set; }
        public string Name { get; set; }
        public string FileType { get; set; }
        public byte[] FileData { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string LinkType { get; set; }
        public string LinkId { get; set; }
    }
}
