namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Staff;

using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public string StaffId { get; set; }

    public StaffCompletionListDto CompletionList { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        CompletionList = await _mediator.Send(new GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery(StaffId));
    }
}
