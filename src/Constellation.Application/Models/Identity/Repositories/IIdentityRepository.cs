namespace Constellation.Application.Models.Identity.Repositories;

using Auth;

public interface IIdentityRepository
{
    Task<List<AppUser>> GetUsers(CancellationToken cancellationToken = default);
    Task<List<AppUser>> UsersInRole(string role, CancellationToken cancellationToken = default);
    Task<List<AppUser>> UsersWithTransientClaim(AuthPermission permission, CancellationToken cancellationToken = default);
    Task AddUserToRole(AppUser user, string role, CancellationToken cancellationToken = default);
    Task<AppUser?> CreateUser(AppUser user, CancellationToken cancellationToken = default);

    Task DeleteUser(AppUser user);

    Task<List<AppRole>> GetRoles(CancellationToken cancellationToken = default);
    Task<AppRole?> GetRole(Guid roleId, CancellationToken cancellationToken = default);
    Task<int> GetUserCountInRole(string roleName, CancellationToken cancellationToken = default);

    Task<List<AuthPermission>> GetRolePermissions(Guid roleId, CancellationToken cancellationToken = default);
}