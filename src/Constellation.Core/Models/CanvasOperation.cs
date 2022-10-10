using System;

namespace Constellation.Core.Models
{
    public abstract class CanvasOperation
    {
        public static class EnrolmentAction
        {
            public const string Add = "Add";
            public const string Remove = "Remove";
        }

        public CanvasOperation()
        {
            CreatedAt = DateTime.Now;
            ScheduledFor = DateTime.Now;
        }

        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ScheduledFor { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateUserCanvasOperation : CanvasOperation
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PortalUsername { get; set; }
        public string EmailAddress { get; set; }
    }

    public class ModifyEnrolmentCanvasOperation : CanvasOperation
    {
        public string UserType { get; set; }
        public string CourseId { get; set; }
        public string Action { get; set; }
    }

    public class DeleteUserCanvasOperation : CanvasOperation
    {
    }
}
