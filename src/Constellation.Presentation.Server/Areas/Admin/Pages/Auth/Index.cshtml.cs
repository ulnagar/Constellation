namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth;

using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public IndexModel(IMediator mediator, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        : base()
    {
        _mediator = mediator;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public int StaffUserCount { get; set; }
    public int SchoolContactUserCount { get; set; }
    public int ParentUserCount { get; set; }

    public List<UserRoleDetailsDto> Roles { get; set; } = new();

    public class UserRoleDetailsDto
    {
        public string Name { get; set; }
        public int MemberCount { get; set; }
    }

    public List<UserDetailsDto> Users { get; set; } = new();

    public class UserDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public async Task<IActionResult> OnGet()
    {
        await GetClasses(_mediator);

        var users = await _userManager.Users.ToListAsync();

        var roles = await _roleManager.Roles.ToListAsync();

        foreach (var role in roles)
        {
            var members = await _userManager.GetUsersInRoleAsync(role.Name);

            Roles.Add(new UserRoleDetailsDto
            {
                Name = role.Name,
                MemberCount = members.Count()
            });
        }

        foreach (var user in users)
        {
            var memberRoles = new List<string>();

            if (user.IsStaffMember)
                memberRoles.Add("Staff");

            if (user.IsSchoolContact)
                memberRoles.Add("SchoolContact");

            if (user.IsParent)
                memberRoles.Add("Parent");

            Users.Add(new UserDetailsDto
            {
                Id = user.Id,
                Name = user.DisplayName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = memberRoles
            });
        }

        StaffUserCount = users.Count(user => user.IsStaffMember);
        SchoolContactUserCount = users.Count(user => user.IsSchoolContact);
        ParentUserCount = users.Count(user => user.IsParent);

        return Page();
    }
}
