namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Modules;

using Application.Common.PresentationModels;
using Application.Training.AddStaffMemberToTrainingModule;
using Application.Training.GetModuleDetails;
using Application.Training.Models;
using Constellation.Application.Models.Auth;
using Constellation.Application.StaffMembers.GetStaffList;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
public class BulkAddMembersModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public BulkAddMembersModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<BulkAddMembersModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Modules;
    [ViewData] public string PageTitle => "Training Module Assignees";


    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(ConstructorBinder))]
    public TrainingModuleId Id { get; set; }

    public ModuleDetailsDto Module { get; set; }
    public List<StaffResponse> StaffMembers { get; set; } = new();

    [BindProperty]
    public List<string> SelectedStaffIds { get; set; } = new();

   
    public async Task OnGet() => await PreparePage();

    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve defaults for adding Members to Training Module by user {User}", _currentUserService.UserName);

        Result<ModuleDetailsDto> moduleRequest = await _mediator.Send(new GetModuleDetailsQuery(Id));

        if (moduleRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), moduleRequest.Error, true)
                .Warning("Failed to retrieve defaults for adding Members to Training Module by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                moduleRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Details", values: new { area = "Staff", Id }));

            return;
        }

        Module = moduleRequest.Value;

        Result<List<StaffResponse>> staffRequest = await _mediator.Send(new GetStaffListQuery());

        if (staffRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), staffRequest.Error, true)
                .Warning("Failed to retrieve defaults for adding Members to Training Module by user {User}", _currentUserService.UserName);
            
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

        AddStaffMemberToTrainingModuleCommand command = new(Id, SelectedStaffIds);

        _logger
            .ForContext(nameof(AddStaffMemberToTrainingModuleCommand), command, true)
            .Information("Requested to add Members to Training Module by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to add Members to Training Module by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(request.Error);

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Training/Modules/Details", new { area = "Staff", Id });
    }
}