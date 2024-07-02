namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Staff;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Training.GetListOfStaffMembersWithoutModule;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
public class WithoutModuleModel : BasePageModel
{
    private readonly ISender _mediator;

    public WithoutModuleModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Staff;
    [ViewData] public string PageTitle => "Staff Training Dashboard";

    public List<StaffResponse> StaffMembers { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<StaffResponse>> response = await _mediator.Send(new GetListOfStaffMembersWithoutModuleQuery());

        if (response.IsFailure)
        {
            ModalContent = new ErrorDisplay(response.Error);

            return;
        }

        StaffMembers = response.Value;
    }
}