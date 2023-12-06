namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Staff;

using Application.Training.Modules.GetListOfCertificatesForStaffMemberWithNotCompletedModules;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Presentation.Server.BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    [ViewData]
    public string StaffId { get; set; }

    public StaffCompletionListDto CompletionList { get; set; }

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Modules;

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Result<StaffCompletionListDto> completionRequest = await _mediator.Send(new GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery(StaffId));

        if (completionRequest.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = completionRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
            };

            return;
        }

        CompletionList = completionRequest.Value;
    }
}
