namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff;

using Application.Common.PresentationModels;
using Constellation.Application.Faculties.GetFacultiesAsDictionary;
using Constellation.Application.Models.Auth;
using Constellation.Application.StaffMembers.GetStaffList;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.Faculties.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Staff;
    [ViewData] public string PageTitle => "Staff List";

    public List<StaffResponse> Staff { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Active;
    
    [BindProperty(SupportsGet = true)]
    public Guid Faculty { get; set; }

    public Dictionary<FacultyId, string> FacultyList = new();

    public async Task OnGet()
    {
        _logger
            .Information("Requested to retrieve list of Staff Members by user {User}", _currentUserService.UserName);

        Result<Dictionary<FacultyId, string>> facultyRequest = await _mediator.Send(new GetFacultiesAsDictionaryQuery());

        if (facultyRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(facultyRequest.Error);

            _logger
                .ForContext(nameof(Error), facultyRequest.Error, true)
                .Warning("Failed to retrieve list of Staff Members by user {User}", _currentUserService.UserName);

            return;
        }

        FacultyList = facultyRequest.Value;

        Result<List<StaffResponse>> request = await _mediator.Send(new GetStaffListQuery());

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve list of Staff Members by user {User}", _currentUserService.UserName);

            return;
        }

        Staff = Filter switch
        {
            FilterDto.Active => request.Value.Where(entry => !entry.IsDeleted).ToList(),
            FilterDto.Inactive => request.Value.Where(entry => entry.IsDeleted).ToList(),
            FilterDto.All => request.Value
        };

        if (Faculty != Guid.Empty)
        {
            FacultyId facultyId = FacultyId.FromValue(Faculty);

            Staff = Staff.Where(entry => entry.Faculties.Any(faculty => faculty.FacultyId == facultyId)).ToList();
        }
    }

    public enum FilterDto
    {
        Active,
        All,
        Inactive
    }
}