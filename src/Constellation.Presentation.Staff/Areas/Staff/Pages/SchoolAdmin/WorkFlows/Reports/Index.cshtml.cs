namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.WorkFlows.Reports;

using Application.Common.PresentationModels;
using Application.DTOs;
using Application.Models.Auth;
using Application.WorkFlows.ExportOpenCaseReport;
using Core.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_WorkFlows_Reports;

    public void OnGet() { }

    public async Task<IActionResult> OnGetDownloadReport()
    {
        AuthorizationResult authorised = await _authService.AuthorizeAsync(User, AuthPolicies.CanManageWorkflows);

        if (!authorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(DomainErrors.Auth.NotAuthorised);

            return Page();
        }

        Result<FileDto> fileRequest = await _mediator.Send(new ExportOpenCaseReportQuery());

        if (fileRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(fileRequest.Error);

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }
}