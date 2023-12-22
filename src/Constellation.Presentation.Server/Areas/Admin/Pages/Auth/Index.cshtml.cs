namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth;

using Constellation.Application.AdminDashboards.AuditUser;
using Constellation.Application.Interfaces.Services;
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
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthService _authService;

    public IndexModel(
        IMediator mediator,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        LinkGenerator linkGenerator,
        IAuthService authService)
        : base()
    {
        _mediator = mediator;
        _userManager = userManager;
        _roleManager = roleManager;
        _linkGenerator = linkGenerator;
        _authService = authService;
    }

    public int StaffUserCount { get; set; }
    public int SchoolContactUserCount { get; set; }
    public int ParentUserCount { get; set; }

    public List<UserRoleDetailsDto> Roles { get; set; } = new();

    public class UserRoleDetailsDto
    {
        public Guid Id { get; set; }
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
        public DateTime? LastLoggedIn { get; set; }

        public bool IsLocked { get; set; }
    }

    public async Task<IActionResult> OnGet()
    {
        var users = await _userManager.Users.ToListAsync();

        var roles = await _roleManager.Roles.ToListAsync();

        foreach (var role in roles)
        {
            var members = await _userManager.GetUsersInRoleAsync(role.Name);

            Roles.Add(new UserRoleDetailsDto
            {
                Id = role.Id,
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

            var locked = await _userManager.GetLockoutEnabledAsync(user);

            Users.Add(new UserDetailsDto
            {
                Id = user.Id,
                Name = user.DisplayName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = memberRoles,
                LastLoggedIn = user.LastLoggedIn,
                IsLocked = locked
            });
        }

        StaffUserCount = users.Count(user => user.IsStaffMember);
        SchoolContactUserCount = users.Count(user => user.IsSchoolContact);
        ParentUserCount = users.Count(user => user.IsParent);

        return Page();
    }

    public async Task<IActionResult> OnGetAudit(Guid userId)
    {
        var result = await _mediator.Send(new AuditUserCommand(userId));

        if (result.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = result.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Auth/Index", values: new { area = "Admin" })
            };

            return Page();
        }

        return RedirectToPage("Index");
    }

    public async Task OnGetAuditAllUsers(CancellationToken cancellationToken = default)
    {
        await _authService.AuditAllUsers(cancellationToken);
    }
}
