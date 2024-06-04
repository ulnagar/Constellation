namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff;

using Constellation.Application.Faculties.GetFacultiesAsDictionary;
using Constellation.Application.Models.Auth;
using Constellation.Application.StaffMembers.GetStaffList;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Models.Faculties.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Staff;

    public List<StaffResponse> Staff { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Active;
    
    [BindProperty(SupportsGet = true)]
    public Guid Faculty { get; set; }

    public Dictionary<FacultyId, string> FacultyList = new();

    public async Task OnGet()
    {
        Result<Dictionary<FacultyId, string>> facultyRequest = await _mediator.Send(new GetFacultiesAsDictionaryQuery());

        if (facultyRequest.IsFailure)
        {
            Error = new()
            {
                Error = facultyRequest.Error,
                RedirectPath = null
            };

            return;
        }

        FacultyList = facultyRequest.Value;

        Result<List<StaffResponse>> request = await _mediator.Send(new GetStaffListQuery());

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

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