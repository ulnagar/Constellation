namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Modules;

using Constellation.Application.Features.MandatoryTraining.Commands;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Models.Auth;
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

    public DetailsModel(IMediator mediator, IDateTimeProvider dateTimeProvider,
        IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _dateTimeProvider = dateTimeProvider;
        _authorizationService = authorizationService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ModuleDetailsDto Module { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Module = await _mediator.Send(new GetModuleDetailsQuery { Id = Id });

        foreach (var record in Module.Completions)
        {
            record.ExpiryCountdown = record.CalculateExpiry();
            record.Status = CompletionRecordDto.ExpiryStatus.Active;

            if (Module.Completions.Any(s => s.ModuleId == record.ModuleId && s.StaffId == record.StaffId && s.CompletedDate > record.CompletedDate))
            {
                record.Status = CompletionRecordDto.ExpiryStatus.Superceded;
            }
        }

        //TODO: Check if return value is null, redirect and display error
    }

    public async Task<IActionResult> OnGetDownloadReport()
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanRunTrainingModuleReports);

        //TODO: Show error explaining why this did not work!
        if (!isAuthorised.Succeeded)
            return Page();

        var report = await _mediator.Send(new GenerateModuleReportCommand { Id = Id });

        return File(report.FileData, report.FileType, report.FileName);
    }

    public async Task<IActionResult> OnGetRetireModule()
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        //TODO: Show error explaining why this did not work!
        if (!isAuthorised.Succeeded)
            return Page();

        var command = new RetireTrainingModuleCommand
        {
            Id = Id,
            DeletedBy = Request.HttpContext.User.Identity?.Name,
            DeletedAt = _dateTimeProvider.Now
        };

        await _mediator.Send(command);

        return RedirectToPage("Details");
    }

    public async Task<IActionResult> OnGetReinstateModule()
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        //TODO: Show error explaining why this did not work!
        if (!isAuthorised.Succeeded)
            return Page();

        var command = new ReinstateTrainingModuleCommand
        {
            Id = Id
        };

        await _mediator.Send(command);

        return RedirectToPage("Details");
    }
}
