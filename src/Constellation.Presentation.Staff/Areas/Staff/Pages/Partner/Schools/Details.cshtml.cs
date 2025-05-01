namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools;

using Application.Common.PresentationModels;
using Application.Domains.ExternalSystems.NetworkStatistics.Queries.GetGraphDataForSchool;
using Application.Domains.Schools.Queries.GetSchoolDetails;
using Application.DTOs;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_Schools;
    [ViewData] public string PageTitle { get; set; } = "School Details";

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }

    public SchoolDetailsResponse School { get; set; }

    public async Task OnGet()
    {
        _logger
            .Information("Requested to retrieve details for School with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<SchoolDetailsResponse> request = await _mediator.Send(new GetSchoolDetailsQuery(Id));

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Partner/Schools/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve details for School with id {Id} by user {User}", Id, _currentUserService.UserName);

            return;
        }

        School = request.Value;
        PageTitle = $"Details - {request.Value.Name}";
    }

    public async Task<IActionResult> OnGetAjaxGetGraphData(string id, int day)
    {
        GraphData data = await _mediator.Send(new GetGraphDataForSchoolQuery { SchoolCode = id, Day = day });

        return new JsonResult(data);
    }
}