using System;

namespace Constellation.Core.Models
{
    public class CanvasAssignmentSubmission
    {
        public Guid Id { get; set; }
        public CanvasAssignment Assignment { get; set; }
        public Guid? AssignmentId { get; set; }
        public Student Student { get; set; }
        public string StudentId { get; set; }
        public DateTime SubmittedDate { get; set; }
        public int Attempt { get; set; }
    }
}
