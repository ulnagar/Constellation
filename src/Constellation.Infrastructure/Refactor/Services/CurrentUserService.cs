namespace Constellation.Infrastructure.Refactor.Services;

using Constellation.Application.Refactor.Common.Interfaces;

public class CurrentUserService : ICurrentUserService
{
    public string UserId { get; set; }
}
