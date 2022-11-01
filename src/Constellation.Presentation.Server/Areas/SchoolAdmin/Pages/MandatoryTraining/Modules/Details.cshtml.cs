using Constellation.Application.Features.MandatoryTraining.Commands;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Modules;

[Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.MandatoryTrainingEditor)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DetailsModel(IMediator mediator, IDateTimeProvider dateTimeProvider)
    {
        _mediator = mediator;
        _dateTimeProvider = dateTimeProvider;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ModuleDetailsDto Module { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Module = await _mediator.Send(new GetModuleDetailsQuery { Id = Id });

        //TODO: Check if return value is null, redirect and display error
    }

    public async Task<IActionResult> OnGetDownloadReport()
    {
        var report = await _mediator.Send(new GenerateModuleReportCommand { Id = Id });

        return File(report.FileData, report.FileType, report.FileName);
    }

    public async Task<IActionResult> OnGetRetireModule()
    {
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
        var command = new ReinstateTrainingModuleCommand
        {
            Id = Id
        };

        await _mediator.Send(command);

        return RedirectToPage("Details");
    }
}
