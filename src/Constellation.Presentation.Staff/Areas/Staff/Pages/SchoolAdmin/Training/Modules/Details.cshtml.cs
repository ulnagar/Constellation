namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Modules;

using Application.Common.PresentationModels;
using Application.Training.Models;
using Constellation.Application.Models.Auth;
using Constellation.Application.Training.Modules.GetModuleDetails;
using Constellation.Application.Training.ReinstateTrainingModule;
using Constellation.Application.Training.RetireTrainingModule;
using Constellation.Core.Errors;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanViewTrainingModuleContentDetails)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IAuthorizationService _authorizationService;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        IMediator mediator,
        IAuthorizationService authorizationService,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ModuleDetailsDto Module { get; set; }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Modules;

    public async Task OnGet()
    {
        Result<ModuleDetailsDto> moduleRequest = await _mediator.Send(new GetModuleDetailsQuery(TrainingModuleId.FromValue(Id)));

        if (moduleRequest.IsFailure)
        {
            Error = new()
            {
                Error = moduleRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" })
            };

            return;
        }

        Module = moduleRequest.Value;

        Module.Completions = Module.Completions
            .OrderByDescending(record => record.CompletedDate)
            .ThenBy(record => record.StaffLastName)
            .ToList();
    }

    public async Task<IActionResult> OnGetRetireModule()
    {
        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!isAuthorised.Succeeded)
        {
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" })
            };

            return Page();
        }

        RetireTrainingModuleCommand command = new RetireTrainingModuleCommand(TrainingModuleId.FromValue(Id));

        await _mediator.Send(command);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetReinstateModule()
    {
        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!isAuthorised.Succeeded)
        {
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Modules/Index", values: new { area = "Staff" })
            };

            return Page();
        }

        ReinstateTrainingModuleCommand command = new ReinstateTrainingModuleCommand(TrainingModuleId.FromValue(Id));

        await _mediator.Send(command);

        return RedirectToPage();
    }
}
