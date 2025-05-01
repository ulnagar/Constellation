namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Modules;

using Application.Common.PresentationModels;
using Application.Domains.Training.Commands.CreateTrainingModule;
using Application.Domains.Training.Commands.UpdateTrainingModule;
using Application.Domains.Training.Queries.GetTrainingModuleEditContext;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
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
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Modules;
    [ViewData] public string PageTitle => Id.Equals(TrainingModuleId.Empty) ? "New Training Module" : "Edit Training Module";


    [BindProperty(SupportsGet = true)]
    public TrainingModuleId Id { get; set; } =  TrainingModuleId.Empty;

    [BindProperty]
    [Required]
    public string Name { get; set; }

    [BindProperty]
    public TrainingModuleExpiryFrequency Expiry { get; set; }

    [BindProperty]
    public string? ModelUrl { get; set; }

    public async Task OnGet()
    {
        if (Id != TrainingModuleId.Empty)
        {
            _logger.Information("Requested to retrieve Training Module with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            // Get existing entry from database and populate fields
            Result<ModuleEditContextDto> entityRequest = await _mediator.Send(new GetTrainingModuleEditContextQuery(Id));

            if (entityRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), entityRequest.Error, true)
                    .Warning("Failed to retrieve Training Module with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    DomainErrors.Permissions.Unauthorised,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" }));

                return;
            }

            ModuleEditContextDto entity = entityRequest.Value;

            Name = entity.Name;
            Expiry = entity.Expiry;
            ModelUrl = entity.Url;
        }
    }

    public async Task<IActionResult> OnPostCreate()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Create new entry
        CreateTrainingModuleCommand command = new(
            Name,
            Expiry,
            ModelUrl);

        _logger
            .ForContext(nameof(CreateTrainingModuleCommand), command, true)
            .Information("Requested to create Training Module by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to create Training Module by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Update existing entry
        UpdateTrainingModuleCommand command = new(
            Id,
            Name,
            Expiry,
            ModelUrl);

        _logger
            .ForContext(nameof(UpdateTrainingModuleCommand), command, true)
            .Information("Requested to update Training Module with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update Training Module with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage("Index");
    }
}
