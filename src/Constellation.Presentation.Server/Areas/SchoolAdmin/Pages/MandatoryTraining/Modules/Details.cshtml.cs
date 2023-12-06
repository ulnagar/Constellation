namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Modules;

using Application.Training.Modules.GenerateModuleReport;
using Application.Training.Modules.GetModuleDetails;
using Application.Training.Modules.ReinstateTrainingModule;
using Application.Training.Modules.RetireTrainingModule;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Errors;
using Constellation.Presentation.Server.BaseModels;
using Core.Models.Training.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanViewTrainingModuleContentDetails)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(IMediator mediator,
        IDateTimeProvider dateTimeProvider,
        IAuthorizationService authorizationService,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _dateTimeProvider = dateTimeProvider;
        _authorizationService = authorizationService;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ModuleDetailsDto Module { get; set; }

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Modules;
    [ViewData] public string StaffId { get; set; }

    public async Task OnGet()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        Result<ModuleDetailsDto> moduleRequest = await _mediator.Send(new GetModuleDetailsQuery(TrainingModuleId.FromValue(Id)));

        if (moduleRequest.IsFailure)
        {
            Error = new()
            {
                Error = moduleRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Index", values: new { area = "SchoolAdmin" })
            };

            return;
        }

        Module = moduleRequest.Value;

        Module.Completions = Module.Completions
            .OrderByDescending(record => record.CompletedDate)
            .ThenBy(record => record.StaffLastName)
            .ToList();
    }

    public async Task<IActionResult> OnGetDownloadReport()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanRunTrainingModuleReports);

        if (!isAuthorised.Succeeded)
        {
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        }

        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateModuleReportCommand(TrainingModuleId.FromValue(Id), false));

        if (reportRequest.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = reportRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Completion/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnGetDownloadReportWithCertificates()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanRunTrainingModuleReports);

        if (!isAuthorised.Succeeded)
        {
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        }

        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateModuleReportCommand(TrainingModuleId.FromValue(Id), true));

        if (reportRequest.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = reportRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Completion/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnGetRetireModule()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!isAuthorised.Succeeded)
        {
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        }

        RetireTrainingModuleCommand command = new RetireTrainingModuleCommand(TrainingModuleId.FromValue(Id));

        await _mediator.Send(command);

        return RedirectToPage("Details");
    }

    public async Task<IActionResult> OnGetReinstateModule()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!isAuthorised.Succeeded)
        {
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        }

        ReinstateTrainingModuleCommand command = new ReinstateTrainingModuleCommand(TrainingModuleId.FromValue(Id));

        await _mediator.Send(command);

        return RedirectToPage("Details");
    }
}
