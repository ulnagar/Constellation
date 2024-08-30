namespace Constellation.Core.Models.Operations;

using Enums;
using System;

public abstract class CanvasOperation
{
    public int Id { get; set; }
    public string UserId { get; protected set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
    public DateTime ScheduledFor { get; protected set; } = DateTime.Now;
    public bool IsCompleted { get; private set; }
    public bool IsDeleted { get; private set; }

    public void Complete() => IsCompleted = true;
    public void Delete() => IsDeleted = true;
}

public class CreateUserCanvasOperation : CanvasOperation
{
    private CreateUserCanvasOperation() { }

    public CreateUserCanvasOperation(
        string userId,
        string firstName,
        string lastName,
        string portalUsername,
        string emailAddress)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        PortalUsername = portalUsername;
        EmailAddress = emailAddress;
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string PortalUsername { get; private set; } = string.Empty;
    public string EmailAddress { get; private set; } = string.Empty;
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

    public CanvasUserType UserType { get; private set; } = CanvasUserType.Student;
    public string CourseId { get; private set; } = string.Empty;
    public CanvasAction Action { get; private set; } = CanvasAction.Add;
}

public class UpdateUserEmailCanvasOperation : CanvasOperation
{
    private UpdateUserEmailCanvasOperation() {}

    public UpdateUserEmailCanvasOperation(
        string userId,
        string portalUsername)
    {
        UserId = userId;
        PortalUsername = portalUsername;
    }

    public string PortalUsername { get; private set; }
}

public class DeleteUserCanvasOperation : CanvasOperation
{
    private  DeleteUserCanvasOperation() { }

    public DeleteUserCanvasOperation(string userId) => UserId = userId;
}