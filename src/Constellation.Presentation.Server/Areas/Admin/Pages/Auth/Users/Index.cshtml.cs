namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth.Users;

using Application.Common.PresentationModels;
using Application.Domains.Auth.Commands.AuditAllUsers;
using Application.Domains.Auth.Commands.AuditUser;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly UserManager<AppUser> _userManager;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        IMediator mediator,
        UserManager<AppUser> userManager,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _userManager = userManager;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Auth_Users;
    [ViewData] public string PageTitle => "Auth Users";

    public int StaffUserCount { get; set; }
    public int SchoolContactUserCount { get; set; }
    public int ParentUserCount { get; set; }
    public int StudentUserCount { get; set; }

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

    [BindProperty(SupportsGet = true)]
    public UserType SelectedUserType { get; set; } = UserType.All;
    
    public enum UserType
    {
        [Display(Name = "All Users")]
        All,
        [Display(Name = "Staff Users")]
        Staff,
        [Display(Name = "School Contact Users")]
        School,
        [Display(Name = "Parent Users")]
        Parent,
        [Display(Name = "Student Users")]
        Student
    }

    public async Task OnGet()
    {
        List<AppUser> users = await _userManager.Users.ToListAsync();
        
        Users = SelectedUserType switch
        {
            UserType.Staff => users.Where(user => user.IsStaffMember).AsEnumerable().Select(ConvertFromAppUser).ToList(),
            UserType.School => users.Where(user => user.IsSchoolContact).AsEnumerable().Select(ConvertFromAppUser).ToList(),
            UserType.Parent => users.Where(user => user.IsParent).AsEnumerable().Select(ConvertFromAppUser).ToList(),
            UserType.Student => users.Where(user => user.IsStudent).AsEnumerable().Select(ConvertFromAppUser).ToList(),
            _ => users.AsEnumerable().Select(ConvertFromAppUser).ToList()
        };

        StaffUserCount = users.Count(user => user.IsStaffMember);
        SchoolContactUserCount = users.Count(user => user.IsSchoolContact);
        ParentUserCount = users.Count(user => user.IsParent);
        StudentUserCount = users.Count(user => user.IsStudent);
    }
        

    private UserDetailsDto ConvertFromAppUser(AppUser user)
    {
        List<string> memberRoles = new();

        if (user.IsStaffMember)
            memberRoles.Add("Staff");

        if (user.IsSchoolContact)
            memberRoles.Add("SchoolContact");

        if (user.IsParent)
            memberRoles.Add("Parent");

        if (user.IsStudent)
            memberRoles.Add("Student");

        return new UserDetailsDto
        {
            Id = user.Id,
            Name = user.DisplayName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = memberRoles,
            LastLoggedIn = user.LastLoggedIn.HasValue ? DateTime.SpecifyKind(user.LastLoggedIn.Value, DateTimeKind.Utc) : null,
        };
    }

    public async Task<IActionResult> OnGetAudit(Guid userId)
    {
        var result = await _mediator.Send(new AuditUserCommand(userId));

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Auth/Users/Index", values: new { area = "Admin" }));

            return Page();
        }

        return RedirectToPage();
    }

    public async Task OnGetAuditAllUsers(CancellationToken cancellationToken = default) => await _mediator.Send(new AuditAllUsersCommand(), cancellationToken);
}
