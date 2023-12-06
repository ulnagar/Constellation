namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Modules;

using Application.Training.Modules.GetListOfModuleSummary;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanViewTrainingModuleContent)]
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

    public List<ModuleSummaryDto> Modules { get; set; } = new();
    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; }

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Modules;
    [ViewData] public string StaffId { get; set; }

    public async Task OnGet()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        Result<List<ModuleSummaryDto>> moduleRequest = await _mediator.Send(new GetListOfModuleSummaryQuery());

        if (moduleRequest.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = moduleRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
            };

            return;
        }

        Modules = moduleRequest.Value;

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
