namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Modules;

using Application.Common.PresentationModels;
using Application.Training.AddStaffMemberToTrainingModule;
using Application.Training.GetModuleDetails;
using Application.Training.Models;
using Constellation.Application.Models.Auth;
using Constellation.Application.StaffMembers.GetStaffList;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.ModelBinders;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
public class BulkAddMembersModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public BulkAddMembersModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Modules;
    [ViewData] public string PageTitle => "Training Module Assignees";


    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public TrainingModuleId Id { get; set; }

    public ModuleDetailsDto Module { get; set; }
    public List<StaffResponse> StaffMembers { get; set; } = new();

    [BindProperty]
    public List<string> SelectedStaffIds { get; set; } = new();

   
    public async Task OnGet() => await PreparePage();

    public async Task PreparePage()
    {
        Result<ModuleDetailsDto> moduleRequest = await _mediator.Send(new GetModuleDetailsQuery(Id));

        if (moduleRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                moduleRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Details", values: new { area = "Staff", Id }));

            return;
        }

        Module = moduleRequest.Value;

        Result<List<StaffResponse>> staffRequest = await _mediator.Send(new GetStaffListQuery());

        if (staffRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                staffRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Details", values: new { area = "Staff", Id }));

            return;
        }

        StaffMembers = staffRequest.Value.Where(member => !member.IsDeleted).ToList();
    }

    public async Task<IActionResult> OnPost()
    {
        if (SelectedStaffIds.Count == 0)
        {
            ModelState.AddModelError("", "You must select at least one Staff Member to add");

            await PreparePage();

            return Page();
        }
        
        Result request = await _mediator.Send(new AddStaffMemberToTrainingModuleCommand(Id, SelectedStaffIds));

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Training/Modules/Details", new { area = "Staff", Id });
    }
}