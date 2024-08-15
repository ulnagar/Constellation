namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Consent.Applications;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.ThirdPartyConsent.GetApplications;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Consent_Applications;
    [ViewData] public string PageTitle => "Consent Applications";

    public List<ApplicationSummaryResponse> Applications { get; set; }

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Active;

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of Consent Applications by user {User}", _currentUserService.UserName);

        Result<List<ApplicationSummaryResponse>> applicationsRequest = await _mediator.Send(new GetApplicationsQuery());

        if (applicationsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), applicationsRequest.Error, true)
                .Warning("Failed to retrieve list of Consent Applications by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(applicationsRequest.Error);

            return;
        }

        Applications = Filter switch
        {
            FilterDto.Active => applicationsRequest.Value.Where(entry => !entry.IsDeleted).ToList(),
            FilterDto.Inactive => applicationsRequest.Value.Where(entry => entry.IsDeleted).ToList(),
            FilterDto.All => applicationsRequest.Value
        };

        Applications = Applications.OrderBy(response => response.Name).ToList();
    }

    public enum FilterDto
    {
        All,
        Active,
        Inactive
    }
}