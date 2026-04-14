namespace Constellation.Core.Models.Auth;

using Enums;

public sealed class AppUserLoginAttempt
{
    private AppUserLoginAttempt() { }

    public AppUserLoginAttempt(
        Guid id,
        DateTime dateTime,
        LoginStatus status)
    {
        AppUserId = id;
        LoginDateTime = dateTime;
        Status = status;
    }

    public Guid AppUserId { get; private set; }
    public DateTime LoginDateTime { get; private set; }
    public LoginStatus Status { get; private set; }
}