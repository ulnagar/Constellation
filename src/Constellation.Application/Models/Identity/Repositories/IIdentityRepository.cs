namespace Constellation.Application.Models.Identity.Repositories;

using Auth;

public interface IIdentityRepository
{
    Task<List<AppUser>> UsersInRole(string role, CancellationToken cancellationToken = default);
    Task<List<AppUser>> UsersWithTransientClaim(AuthPermission permission, CancellationToken cancellationToken = default);

    Task<List<AppRole>> GetRoles(CancellationToken cancellationToken = default);
    Task<int> GetUserCountInRole(string roleName, CancellationToken cancellationToken = default);
}