using Constellation.Application.Features.MandatoryTraining.Commands;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining;

[Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.MandatoryTrainingEditor)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;

    public DetailsModel(IMediator mediator)
    {
        _mediator = mediator;
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

    public async Task<IActionResult> OnPostDownloadReport()
    {
        var report = await _mediator.Send(new GenerateModuleReportCommand { Id = Id });

        return File(report.FileData, report.FileType, report.FileName);
    }
}
