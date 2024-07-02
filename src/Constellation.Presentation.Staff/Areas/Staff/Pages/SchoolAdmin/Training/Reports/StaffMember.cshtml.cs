namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Reports;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.StaffMembers.GetStaffById;
using Constellation.Application.Training.GetModuleStatusByStaffMember;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanRunTrainingModuleReports)]
public class StaffMemberModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public StaffMemberModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Reports;
    [ViewData] public string PageTitle => StaffMember is null ? "Staff Training Status" : $"Training Status - {StaffMember.Name.DisplayName}";


    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }

    public List<ModuleStatusResponse> Modules { get; set; } = new();
    public StaffResponse? StaffMember { get; set; }
    
    public async Task OnGet()
    {
        Result<StaffResponse> staffRequest = await _mediator.Send(new GetStaffByIdQuery(Id));

        if (staffRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                staffRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Reports/Index", values: new { area = "Staff" }));

            return;
        }

        StaffMember = staffRequest.Value;

        Result<List<ModuleStatusResponse>> moduleRequest = await _mediator.Send(new GetModuleStatusByStaffMemberQuery(Id));

        if (moduleRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                moduleRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Reports/Index", values: new { area = "Staff" }));

            return;
        }

        Modules = moduleRequest.Value;
    }
}