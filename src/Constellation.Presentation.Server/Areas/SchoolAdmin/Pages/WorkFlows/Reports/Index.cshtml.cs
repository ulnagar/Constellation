namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.WorkFlows.Reports;

using Application.DTOs;
using Application.Models.Auth;
using Application.WorkFlows.ExportOpenCaseReport;
using BaseModels;
using Core.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Workflows;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authService;

    public IndexModel(
        ISender mediator,
        IAuthorizationService authService)
    {
        _mediator = mediator;
        _authService = authService;
    }

    [ViewData] public string ActivePage => WorkFlowPages.Reports;

    public void OnGet() { }

    public async Task<IActionResult> OnGetDownloadReport()
    {
        AuthorizationResult authorised = await _authService.AuthorizeAsync(User, AuthPolicies.CanManageWorkflows);

        if (!authorised.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Auth.NotAuthorised,
                RedirectPath = null
            };

            return Page();
        }

        Result<FileDto> fileRequest = await _mediator.Send(new ExportOpenCaseReportQuery());

        if (fileRequest.IsFailure)
        {
            Error = new()
            {
                Error = fileRequest.Error,
                RedirectPath = null
            };

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }
}