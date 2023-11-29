namespace Constellation.Presentation.Server.Areas.Partner.Pages.Staff;

using Application.Faculties.GetFacultiesAsDictionary;
using Application.Models.Auth;
using Application.StaffMembers.GetStaffList;
using BaseModels;
using Core.Models.Faculty.Identifiers;
using Core.Shared;
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

    public List<StaffResponse> Staff { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Active;
    
    [BindProperty(SupportsGet = true)]
    public Guid Faculty { get; set; }

    public Dictionary<FacultyId, string> FacultyList = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

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

        if (Faculty != Guid.NewGuid())
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