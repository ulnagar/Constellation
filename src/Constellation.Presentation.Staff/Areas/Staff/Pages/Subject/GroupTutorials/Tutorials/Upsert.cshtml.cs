namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.GroupTutorials.Tutorials;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.GroupTutorials.CreateGroupTutorial;
using Constellation.Application.GroupTutorials.EditGroupTutorial;
using Constellation.Application.GroupTutorials.GetTutorialById;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditGroupTutorials)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator, 
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_GroupTutorials_Tutorials;
    [ViewData] public string PageTitle { get; set; } = "New Group Tutorial";

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(ConstructorBinder))]
    public GroupTutorialId Id { get; set; } = GroupTutorialId.Empty;

    [BindProperty]
    public string Name { get; set; }

    [BindProperty]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [BindProperty]
    public DateTime EndDate { get; set; } = DateTime.Today;

    public async Task OnGet()
    {
        if (Id != GroupTutorialId.Empty)
        {
            _logger.Information("Requested to retrieve Group Tutorial with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            Result<GroupTutorialResponse> entry = await _mediator.Send(new GetTutorialByIdQuery(GroupTutorialId.FromValue(Id.Value)));

            if (entry.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), entry.Error, true)
                    .Warning("Failed to retrieve Group Tutorial with id {Id} for edit by user {User}", Id, _currentUserService.UserName);
                
                ModalContent = new ErrorDisplay(
                    entry.Error,
                    _linkGenerator.GetPathByPage("/Subject/GroupTutorials/Tutorials/Index", values: new { area = "Staff" }));

                return;
            }

            Name = entry.Value.Name;
            StartDate = entry.Value.StartDate.ToDateTime(TimeOnly.MinValue);
            EndDate = entry.Value.EndDate.ToDateTime(TimeOnly.MinValue);

            PageTitle = $"Edit - {Name}";
        }
    }

    public async Task<IActionResult> OnPostUpdate(CancellationToken cancellationToken)
    {
        if (Id != GroupTutorialId.Empty)
        {
            EditGroupTutorialCommand command = new(
                Id,
                Name,
                DateOnly.FromDateTime(StartDate),
                DateOnly.FromDateTime(EndDate));

            _logger
                .ForContext(nameof(EditGroupTutorialCommand), command, true)
                .Information("Requested to update Group Tutorial by user {User}", _currentUserService.UserName);

            Result result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to update Group Tutorial by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(result.Error);

                return Page();
            }
        }
        else
        {
            CreateGroupTutorialCommand command = new(
                Name,
                DateOnly.FromDateTime(StartDate),
                DateOnly.FromDateTime(EndDate));

            _logger
                .ForContext(nameof(CreateGroupTutorialCommand), command, true)
                .Information("Requested to create new Group Tutorial by user {User}", _currentUserService.UserName);

            Result<GroupTutorialId> result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure && result is ValidationResult<GroupTutorialId> validationResult)
            {
                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to create new Group Tutorial by user {User}", _currentUserService.UserName);

                foreach (Error error in validationResult.Errors)
                    ModelState.AddModelError(error.Code, error.Message);

                return Page();
            }
            
            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to create new Group Tutorial by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(result.Error);

                return Page();
            }
        }

        return RedirectToPage("/Subject/GroupTutorials/Tutorials/Index", new { area = "Staff" });
    }
}
