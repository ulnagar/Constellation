namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Modules;

using Application.Common.PresentationModels;
using Application.Training.GetListOfModuleSummary;
using Application.Training.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Modules;
    [ViewData] public string PageTitle => "Training Modules";

    public List<ModuleSummaryDto> Modules { get; set; } = new();

    [BindProperty(SupportsGet = true)] 
    public FilterDto Filter { get; set; } = FilterDto.Active;

    public async Task OnGet()
    {
        Result<List<ModuleSummaryDto>> moduleRequest = await _mediator.Send(new GetListOfModuleSummaryQuery());

        if (moduleRequest.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = moduleRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" })
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
