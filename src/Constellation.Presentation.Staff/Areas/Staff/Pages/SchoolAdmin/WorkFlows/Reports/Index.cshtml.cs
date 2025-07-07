namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.WorkFlows.Reports;

using Application.Common.PresentationModels;
using Application.Domains.WorkFlows.Queries.ExportOpenCaseReport;
using Application.DTOs;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        IAuthorizationService authService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _authService = authService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_WorkFlows_Reports;
    [ViewData] public string PageTitle => "WorkFlow Reports";

    public void OnGet() { }

    public async Task<IActionResult> OnGetDownloadReport()
    {
        _logger.Information("Requested to generate Open Case Report for WorkFlow Cases by user {User}", _currentUserService.UserName);

        AuthorizationResult authorised = await _authService.AuthorizeAsync(User, AuthPolicies.CanManageWorkflows);

        if (!authorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Auth.NotAuthorised, true)
                .Information("Requested to generate Open Case Report for WorkFlow Cases by user {User}", _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(DomainErrors.Auth.NotAuthorised);

            return Page();
        }

        Result<FileDto> fileRequest = await _mediator.Send(new ExportOpenCaseReportQuery());

        if (fileRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), fileRequest.Error, true)
                .Warning("Failed to generate Open Case Report for WorkFlow Cases by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(fileRequest.Error);

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }
}