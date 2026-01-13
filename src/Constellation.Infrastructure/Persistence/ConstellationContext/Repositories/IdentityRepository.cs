namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Application.Models.Auth;
using Application.Models.Identity.Repositories;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class IdentityRepository : IIdentityRepository
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly AppDbContext _context;

    public IdentityRepository(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<List<AppUser>> UsersInRole(
        string roleName,
        CancellationToken cancellationToken = default)
    {
        IList<AppUser> users = await _userManager.GetUsersInRoleAsync(roleName);

        return users.ToList();
    }

    public async Task<List<AppUser>> UsersWithTransientClaim(
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

    public async Task<List<AppRole>> GetRoles(
        CancellationToken cancellationToken = default) =>
        await _roleManager.Roles.ToListAsync(cancellationToken);

    public async Task<int> GetUserCountInRole(
        string roleName,
        CancellationToken cancellationToken = default)
    {
        IList<AppUser> users = await _userManager.GetUsersInRoleAsync(roleName);
        return users.Count;
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
}