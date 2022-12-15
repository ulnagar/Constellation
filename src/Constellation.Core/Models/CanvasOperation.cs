namespace Constellation.Core.Models;

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
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ScheduledFor { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsDeleted { get; set; }
}

public class CreateUserCanvasOperation : CanvasOperation
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PortalUsername { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
}

public class ModifyEnrolmentCanvasOperation : CanvasOperation
{
    public string UserType { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public class DeleteUserCanvasOperation : CanvasOperation
{
}
