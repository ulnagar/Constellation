namespace Constellation.Application.Models.Identity.Repositories;

using Auth;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Domains.Auth.Queries.GetFilteredUsers;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading;

public interface IIdentityRepository
{
    Task<AppUser?> GetUser(Guid Id, CancellationToken cancellationToken = default);
    Task<AppUser?> GetUserByEmail(string email, CancellationToken cancellationToken = default);
    Task<List<AppUser>> GetUsers(CancellationToken cancellationToken = default);
    Task<List<AppUser>> GetUsersInRole(string role, CancellationToken cancellationToken = default);
    Task<List<AppUser>> GetUsersWithTransientClaim(AuthPermission permission, CancellationToken cancellationToken = default);
    Task<List<AppUser>> GetFilteredUsers(UserFilter filter, CancellationToken cancellationToken = default);
    Task<IdentityResult> AddUserToRole(AppUser user, string role, CancellationToken cancellationToken = default);
    Task<AppUser?> CreateUser(AppUser user, CancellationToken cancellationToken = default);

    Task DeleteUser(AppUser user);

    Task<List<AppRole>> GetRoles(CancellationToken cancellationToken = default);
    Task<AppRole?> GetRole(Guid roleId, CancellationToken cancellationToken = default);
    Task<int> GetUserCountInRole(string roleName, CancellationToken cancellationToken = default);
    Task<List<AppRole>> GetRolesForUser(AppUser user, CancellationToken cancellationToken = default);

    Task<List<AuthPermission>> GetRolePermissions(Guid roleId, CancellationToken cancellationToken = default);
    Task<IdentityResult> AddPermissionToRole(AppRole role, AuthPermission permission, CancellationToken cancellationToken = default);
    Task<IdentityResult> RemovePermissionFromRole(AppRole role, AuthPermission permission, CancellationToken cancellationToken = default);

    Task<AppRole?> AddRole(AppRole role, CancellationToken cancellationToken = default);
    Task<List<Claim>> GetClaims(AppUser user, CancellationToken cancellationToken = default);

    Task<AppUserPasskey?> GetPasskeyById(byte[] id, CancellationToken cancellationToken = default);
    Task<bool> DoesCredentialAlreadyExist(byte[] id, CancellationToken cancellationToken = default);
    void Insert(AppUserPasskey passkey);

    Task<bool> UserHasOptedInToNotification(string email, NotificationType notificationType, CancellationToken cancellationToken = default);
    Task<bool> UserHasOptedInToNotification(Guid id, NotificationType notificationType, CancellationToken cancellationToken = default);
    Task<List<NotificationType>> GetOptedInNotificationTypesForUser(Guid id, CancellationToken cancellationToken = default);
    void Insert(AppUserNotificationPreference preference);
    void Remove(AppUserNotificationPreference preference);
}