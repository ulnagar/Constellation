using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Modules;

[Roles(AuthRoles.Admin, AuthRoles.User, AuthRoles.MandatoryTrainingEditor)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<ModuleSummaryDto> Modules { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Modules = await _mediator.Send(new GetListOfModuleSummaryQuery());
    }
}
