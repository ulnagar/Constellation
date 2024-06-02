namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff;

using Application.Models.Auth;
using Application.StaffMembers.GetLifecycleDetailsForStaffMember;
using Application.StaffMembers.GetStaffDetails;
using Constellation.Application.Students.GetLifecycleDetailsForStudent;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Staff;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    public StaffDetailsResponse StaffMember { get; set; }

    public RecordLifecycleDetailsResponse RecordLifecycle { get; set; }

    public async Task OnGet()
    {
        Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));

        if (staffRequest.IsFailure)
        {
            Error = new()
            {
                Error = staffRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Staff/Index", values: new { area = "Staff" })
            };

            return;
        }

        StaffMember = staffRequest.Value;

        Result<RecordLifecycleDetailsResponse> recordLifecycle = await _mediator.Send(new GetLifecycleDetailsForStaffMemberQuery(Id));

        RecordLifecycle = recordLifecycle.IsSuccess
            ? recordLifecycle.Value
            : new RecordLifecycleDetailsResponse(
                string.Empty,
                DateTime.MinValue,
                string.Empty,
                DateTime.MinValue,
                string.Empty,
                null);
    }
}