namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Application.Models.Auth;
using Application.Models.Identity.Enums;
using Application.Models.Identity.Repositories;
using Constellation.Application.Domains.Auth.Queries.GetFilteredUsers;
using Constellation.Application.Models.Identity;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

public sealed class IdentityRepository : IIdentityRepository
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ConstellationDbContext _context;

    public IdentityRepository(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        ConstellationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<AppUser?> GetUser(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await _userManager.Users
            .FirstOrDefaultAsync(user => 
                user.Id == id, 
                cancellationToken);

    public async Task<List<AppUser>> GetUsers(
        CancellationToken cancellationToken = default) =>
        _userManager.Users.ToList();

    public async Task<List<AppUser>> GetUsersInRole(
        string roleName,
        CancellationToken cancellationToken = default)
    {
        IList<AppUser> users = await _userManager.GetUsersInRoleAsync(roleName);

        return users.ToList();
    }

    public async Task<List<AppUser>> GetUsersWithTransientClaim(
        AuthPermission permission,
        CancellationToken cancellationToken = default)
    {
        List<Guid> roleIds = await _context
            .Set<AppRole>()
            .Where(role => _context.RoleClaims
                .Any(roleClaim => 
                    roleClaim.RoleId == role.Id &&
                    roleClaim.ClaimType == AuthClaimType.Permission &&
                    roleClaim.ClaimValue == permission))
            .Select(role => role.Id)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<AppUser>()
            .Where(user => _context.UserRoles
                .Any(userRole =>
                    userRole.UserId == user.Id &&
                    roleIds.Contains(userRole.RoleId)))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AppUser>> GetFilteredUsers(
        UserFilter filter,
        CancellationToken cancellationToken = default) =>
        filter switch
        {
            UserFilter.Staff => 
                await _context
                    .Set<AppUser>()
                    .Where(user => user.Links.Any(link => 
                        !link.IsDeleted && 
                        link.Type == LinkType.Staff))
                    .ToListAsync(cancellationToken),
            UserFilter.Student => 
                await _context
                    .Set<AppUser>()
                    .Where(user => user.Links.Any(link =>
                        !link.IsDeleted &&
                        link.Type == LinkType.Student))
                    .ToListAsync(cancellationToken),
            UserFilter.Family => 
                await _context
                    .Set<AppUser>()
                    .Where(user => user.Links.Any(link =>
                        !link.IsDeleted &&
                        link.Type == LinkType.Family))
                    .ToListAsync(cancellationToken),
            UserFilter.Parent => 
                await _context
                    .Set<AppUser>()
                    .Where(user => user.Links.Any(link =>
                        !link.IsDeleted &&
                        link.Type == LinkType.Parent))
                    .ToListAsync(cancellationToken),
            UserFilter.School => 
                await _context
                    .Set<AppUser>()
                    .Where(user => user.Links.Any(link =>
                        !link.IsDeleted &&
                        link.Type == LinkType.Contact))
                    .ToListAsync(cancellationToken),
            _ => await _context
                .Set<AppUser>()
                .ToListAsync(cancellationToken)
        };

    public async Task<List<AppRole>> GetRoles(
        CancellationToken cancellationToken = default) =>
        await _roleManager.Roles.ToListAsync(cancellationToken);

    public async Task<AppRole?> GetRole(
        Guid roleId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AppRole>()
            .FirstOrDefaultAsync(role => role.Id == roleId, cancellationToken);

    public async Task<int> GetUserCountInRole(
        string roleName,
        CancellationToken cancellationToken = default)
    {
        IList<AppUser> users = await _userManager.GetUsersInRoleAsync(roleName);
        return users.Count;
    }

    public async Task<List<AuthPermission>> GetRolePermissions(
        Guid roleId,
        CancellationToken cancellationToken = default) =>
        await _context
            .RoleClaims
            .Where(claim => 
                claim.RoleId == roleId &&
                claim.ClaimType == AuthClaimType.Permission)
            .Select(claim => AuthPermission.FromValue(claim.ClaimValue))
            .ToListAsync(cancellationToken);

    public async Task DeleteUser(AppUser user) => 
        await _userManager.DeleteAsync(user);

    public async Task<IdentityResult> AddUserToRole(
        AppUser user,
        string roleName,
        CancellationToken cancellationToken = default) =>
        await _userManager
            .AddToRoleAsync(user, roleName);

    public async Task<AppUser?> CreateUser(
        AppUser user, 
        CancellationToken cancellationToken = default)
    {
        IdentityResult created = await _userManager.CreateAsync(user);

        if (!created.Succeeded)
            return null;

        return await _context
            .Set<AppUser>()
            .FirstOrDefaultAsync(
                entry => entry.Email == user.Email,
                cancellationToken);
    }

    public async Task<List<AppRole>> GetRolesForUser(
        AppUser user,
        CancellationToken cancellationToken = default)
    {
        List<Guid> roleIds = await _context
            .UserRoles
                .Where(userRole => userRole.UserId == user.Id)
                .Select(userRole => userRole.RoleId)
                .ToListAsync(cancellationToken);

        return await _context
            .Set<AppRole>()
            .Where(role => roleIds.Contains(role.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IdentityResult> AddPermissionToRole(
        AppRole role,
        AuthPermission permission,
        CancellationToken cancellationToken = default) =>
        await _roleManager.AddClaimAsync(role, new Claim(AuthClaimType.Permission, permission));

    public async Task<IdentityResult> RemovePermissionFromRole(
        AppRole role,
        AuthPermission permission,
        CancellationToken cancellationToken = default)
    {
        IList<Claim> claims = await _roleManager.GetClaimsAsync(role);

        Claim? claim = claims.FirstOrDefault(claim => 
            claim.Type == AuthClaimType.Permission && 
            claim.Value == permission);

        if (claim is null)
            return new IdentityResult();

        return await _roleManager.RemoveClaimAsync(role, claim);
    }

    public async Task<AppRole?> AddRole(
        AppRole role,
        CancellationToken cancellationToken = default)
    {
        IdentityResult created = await _roleManager.CreateAsync(role);

        if (!created.Succeeded)
            return null;

        return await _context
            .Set<AppRole>()
            .FirstOrDefaultAsync(
                entry => entry.Name == role.Name,
                cancellationToken);
    }

    public async Task<List<Claim>> GetClaims(
        AppUser user,
        CancellationToken cancellationToken = default)
    {
        IList<Claim> claims = await _userManager.GetClaimsAsync(user);

        return claims.ToList();
    }
}