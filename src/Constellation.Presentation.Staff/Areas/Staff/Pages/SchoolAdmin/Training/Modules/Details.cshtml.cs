namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Modules;

using Application.Common.PresentationModels;
using Application.Domains.Training.Commands.AddStaffMemberToTrainingModule;
using Application.Domains.Training.Commands.ReinstateTrainingModule;
using Application.Domains.Training.Commands.RemoveStaffMemberToTrainingModule;
using Application.Domains.Training.Commands.RetireTrainingModule;
using Application.Domains.Training.Models;
using Application.Domains.Training.Queries.GetModuleDetails;
using Constellation.Application.Models.Auth;
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
using Shared.Components.AddStaffMemberToTrainingModule;
using Shared.PartialViews.RemoveStaffMemberFromTrainingModuleModal;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanViewTrainingModuleContentDetails)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authorizationService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        IAuthorizationService authorizationService,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Modules;
    [ViewData] public string PageTitle => Module is not null ? $"Training Module - {Module.Name}" : "Training Module Details";


    [BindProperty(SupportsGet = true)]
    public TrainingModuleId Id { get; set; }

    public ModuleDetailsDto? Module { get; set; }
    
    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnGetRetireModule()
    {
        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!isAuthorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Permissions.Unauthorised,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" }));

            await PreparePage();
            
            return Page();
        }

        RetireTrainingModuleCommand command = new(Id);

        _logger.Information("Requested to retire Training Module with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to retire Training Module with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetReinstateModule()
    {
        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!isAuthorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Permissions.Unauthorised,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" }));

            await PreparePage();
            
            return Page();
        }

        ReinstateTrainingModuleCommand command = new(Id);

        _logger.Information("Requested to reinstate Training Module with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to reinstate Training Module with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxRemoveMember(
        string staffId,
        string staffName, 
        string moduleName)
    {
        RemoveStaffMemberFromTrainingModuleModalViewModel viewModel = new(
            staffId,
            staffName,
            moduleName);

        return Partial("RemoveStaffMemberFromTrainingModuleModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveStaff(string staffId)
    {
        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!isAuthorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Permissions.Unauthorised,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" }));

            await PreparePage();

            return Page();
        }

        RemoveStaffMemberToTrainingModuleCommand command = new(
            Id,
            staffId);

        _logger
            .ForContext(nameof(RemoveStaffMemberToTrainingModuleCommand), command, true)
            .Information("Requested to remove member from Training Module by user {User}", _currentUserService.UserName);

        Result response = await _mediator.Send(command);

        if (response.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), response.Error, true)
                .Warning("Failed to remove member from Training Module by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(response.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddStaffMember(AddStaffMemberToTrainingModuleSelection viewModel)
    {
        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!isAuthorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Permissions.Unauthorised,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" }));

            await PreparePage();

            return Page();
        }

        AddStaffMemberToTrainingModuleCommand command = new(
            Id,
            new() { viewModel.StaffId });

        _logger
            .ForContext(nameof(AddStaffMemberToTrainingModuleCommand), command, true)
            .Information("Requested to add member to Training Module by user {User}", _currentUserService.UserName);

        Result response = await _mediator.Send(command);

        if (response.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), response.Error, true)
                .Warning("Failed to add member to Training Module by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(response.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve details of Training Module with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<ModuleDetailsDto> moduleRequest = await _mediator.Send(new GetModuleDetailsQuery(Id));

        if (moduleRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), moduleRequest.Error, true)
                .Warning("Failed to retrieve details of Training Module with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                moduleRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" }));

            return;
        }

        Module = moduleRequest.Value;
    }
}
