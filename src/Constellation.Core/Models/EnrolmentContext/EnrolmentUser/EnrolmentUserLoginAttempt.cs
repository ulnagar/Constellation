namespace Constellation.Core.Models.EnrolmentContext.EnrolmentUser;

using Auth.Enums;

public sealed class EnrolmentUserLoginAttempt
{
    private EnrolmentUserLoginAttempt() { }

    public EnrolmentUserLoginAttempt(
        Guid id,
        DateTime dateTime,
        LoginStatus status)
    {
        UserId = id;
        LoginDateTime = dateTime;
        Status = status;
    }

    public Guid UserId { get; private set; }
    public DateTime LoginDateTime { get; private set; }
    public LoginStatus Status { get; private set; }
}