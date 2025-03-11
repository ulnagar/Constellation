namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Modules;

using Application.Common.PresentationModels;
using Application.Training.GetListOfModuleSummary;
using Application.Training.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanViewTrainingModuleContent)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Modules;
    [ViewData] public string PageTitle => "Training Modules";

    public List<ModuleSummaryDto> Modules { get; set; } = new();

    [BindProperty(SupportsGet = true)] 
    public FilterDto Filter { get; set; } = FilterDto.Active;

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of Training Modules by user {User}", _currentUserService.UserName);

        Result<List<ModuleSummaryDto>> moduleRequest = await _mediator.Send(new GetListOfModuleSummaryQuery());

        if (moduleRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), moduleRequest.Error, true)
                .Warning("Failed to retrieve list of Training Modules by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                moduleRequest.Error,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

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
