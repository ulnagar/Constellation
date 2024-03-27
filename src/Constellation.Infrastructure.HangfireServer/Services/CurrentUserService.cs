namespace Constellation.Infrastructure.HangfireServer.Services;

using Core.Abstractions.Services;

public class CurrentUserService : ICurrentUserService
{
    public string? UserName => "HANGFIRE";

    public string EmailAddress => "system@aurora.nsw.edu.au";

    public bool IsAuthenticated => true;
}
