namespace Constellation.Core.Models.Operations;

using Enums;
using System;

public abstract class CanvasOperation
{
    public int Id { get; set; }
    public string UserId { get; protected set; }
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

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string PortalUsername { get; private set; }
    public string EmailAddress { get; private set; }
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
    private  DeleteUserCanvasOperation() { }

    public DeleteUserCanvasOperation(string userId) => UserId = userId;
}