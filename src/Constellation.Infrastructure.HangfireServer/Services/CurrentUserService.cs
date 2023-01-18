namespace Constellation.Infrastructure.HangfireServer.Services;

using Constellation.Application.Interfaces.Services;

public class CurrentUserService : ICurrentUserService
{
    public string? UserName => "HANGFIRE";

    public bool IsAuthenticated => true;
}
