namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Modules;

using Constellation.Application.Interfaces.Providers;
using Constellation.Application.MandatoryTraining.GenerateModuleReport;
using Constellation.Application.MandatoryTraining.GetModuleDetails;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Application.MandatoryTraining.ReinstateTrainingModule;
using Constellation.Application.MandatoryTraining.RetireTrainingModule;
using Constellation.Application.Models.Auth;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Server.BaseModels;
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

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        var moduleRequest = await _mediator.Send(new GetModuleDetailsQuery(TrainingModuleId.FromValue(Id)));

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

        foreach (var record in Module.Completions)
        {
            record.ExpiryCountdown = record.CalculateExpiry();
            record.Status = CompletionRecordDto.ExpiryStatus.Active;

            if (Module.Completions.Any(other =>
                other.Id != record.Id &&
                other.ModuleId == record.ModuleId && // true
                other.StaffId == record.StaffId && // true
                ((other.NotRequired && other.CreatedAt > record.CompletedDate.Value) || // false
                (!other.NotRequired && !record.NotRequired && other.CompletedDate.Value > record.CompletedDate.Value) || // false
                (record.NotRequired && record.CreatedAt < other.CompletedDate.Value))))
            {
                record.Status = CompletionRecordDto.ExpiryStatus.Superceded;
            }
        }

        Module.Completions = Module.Completions
            .OrderByDescending(record => record.CompletedDate)
            .ThenBy(record => record.StaffLastName)
            .ToList();
    }

    public async Task<IActionResult> OnGetDownloadReport()
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanRunTrainingModuleReports);

        if (!isAuthorised.Succeeded)
        {
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        }

        var reportRequest = await _mediator.Send(new GenerateModuleReportCommand(TrainingModuleId.FromValue(Id), false));

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
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanRunTrainingModuleReports);

        if (!isAuthorised.Succeeded)
        {
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        }

        var reportRequest = await _mediator.Send(new GenerateModuleReportCommand(TrainingModuleId.FromValue(Id), true));

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
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!isAuthorised.Succeeded)
        {
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        }

        var command = new RetireTrainingModuleCommand(TrainingModuleId.FromValue(Id));

        await _mediator.Send(command);

        return RedirectToPage("Details");
    }

    public async Task<IActionResult> OnGetReinstateModule()
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!isAuthorised.Succeeded)
        {
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/MandatoryTraining/Modules/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        }

        var command = new ReinstateTrainingModuleCommand(TrainingModuleId.FromValue(Id));

        await _mediator.Send(command);

        return RedirectToPage("Details");
    }
}
