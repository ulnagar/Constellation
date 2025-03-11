namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff.Faculties;

using Constellation.Application.Faculties.GetFaculty;
using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Models.Auth;
using Constellation.Application.StaffMembers.AddStaffToFaculty;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditFaculties)]
public class AddMemberModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AddMemberModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AddMemberModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    public class AddMemberDto
    {
        [Required]
        public Guid? FacultyId { get; set; }
        public string? FacultyName { get; set; }
        [Required]
        public string StaffId { get; set; }
        [Required]
        public string Role { get; set; }
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Faculties;
    [ViewData] public string PageTitle => "Add Member to Faculty";

    [BindProperty(SupportsGet = true)]
    public Guid FacultyId { get; set; }

    [BindProperty]
    public AddMemberDto MemberDefinition { get; set; } = new();

    public Dictionary<string, string> StaffList { get; set; } = new();

    public SelectList FacultyRoles { get; set; } = new(FacultyMembershipRole.Enumerations(), "");

    public async Task OnGet()
    {
        _logger.Information("Requested to add member to Faculty with Id {FaultyId} by user {User}", FacultyId, _currentUserService.UserName);

        StaffList = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());
        MemberDefinition.FacultyId = FacultyId;
        
        FacultyId facultyId = Core.Models.Faculties.Identifiers.FacultyId.FromValue(FacultyId);

        Result<FacultyResponse> faculty = await _mediator.Send(new GetFacultyQuery(facultyId));

        if (faculty.IsSuccess)
            MemberDefinition.FacultyName = faculty.Value.Name;
    }

    public async Task<IActionResult> OnPostAddMember()
    {
        if (!ModelState.IsValid)
        {
            StaffList = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

            return Page();
        }

        if (MemberDefinition.FacultyId.HasValue)
        {
            FacultyMembershipRole role = FacultyMembershipRole.FromValue(MemberDefinition.Role);

            FacultyId facultyId = Core.Models.Faculties.Identifiers.FacultyId.FromValue(MemberDefinition.FacultyId.Value);

            AddStaffToFacultyCommand command = new(
                MemberDefinition.StaffId,
                facultyId,
                role);

            _logger
                .ForContext(nameof(AddStaffToFacultyCommand), command, true)
                .Information("Requested to add member to Faculty with Id {FacultyId} by user {User}", FacultyId, _currentUserService.UserName);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to add member to Faculty with id {FacultyId} by user {User}", FacultyId, _currentUserService.UserName);
            }
        }
        
        return RedirectToPage("/Partner/Staff/Faculties/Details", new { FacultyId = MemberDefinition.FacultyId, area="Staff"});
    }
}
