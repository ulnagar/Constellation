namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Modules;

using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanViewTrainingModuleContent)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<ModuleSummaryDto> Modules { get; set; } = new();
    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Modules = await _mediator.Send(new GetListOfModuleSummaryQuery());

        Modules = Filter switch
        {
            FilterDto.All => Modules,
            FilterDto.Active => Modules.Where(module => module.IsActive).ToList(),
            FilterDto.Inactive => Modules.Where(module => !module.IsActive).ToList(),
            _ => Modules
        };

        Modules = Modules.OrderBy(module => module.Name).ToList();
    }

    public enum FilterDto
    {
        All,
        Active,
        Inactive
    }
}
