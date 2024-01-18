using System;

namespace Constellation.Core.Models.Operations
{
    using Enums;

    public abstract class CanvasOperation
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ScheduledFor { get; set; } = DateTime.Now;
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
        private ModifyEnrolmentCanvasOperation() {}

        public ModifyEnrolmentCanvasOperation(
            string userId,
            string courseId,
            CanvasAction action,
            CanvasUserType userType,
            DateTime? scheduledFor)
        {
            UserId = userId;
            CourseId = courseId;
            Action = action;
            UserType = userType;

            if (scheduledFor.HasValue)
            {
                ScheduledFor = scheduledFor.Value;
            }
        }

        public CanvasUserType UserType { get; private set; }
        public string CourseId { get; private set; }
        public CanvasAction Action { get; private set; }
    }

    public class DeleteUserCanvasOperation : CanvasOperation
    {
    }
}
