namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Compliance.AssessmentProvisions;

using Application.Domains.Compliance.Assessments.Commands.ImportProvisionsFromFile;
using Application.Domains.Compliance.Assessments.Commands.SendNotificationsForAssessmentProvisions;
using Application.Domains.Compliance.Assessments.Interfaces;
using Application.Domains.Compliance.Assessments.Models;
using Application.Helpers;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAssessmentProvisionsCacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IAssessmentProvisionsCacheService cacheService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Compliance_AssessmentProvisions;
    [ViewData] public string PageTitle => "Assessment Provisions";


    public List<StudentProvisions> Provisions { get; set; } = [];
    public void OnGet()
    {
        Provisions = _cacheService.GetRecords().ToList();
    }

    public async Task<IActionResult> OnPost([FromForm] IFormCollection formData)
    {
        if (formData is null || formData.Files.Count == 0)
        {
            var error = new Error("Page Upload", "You must select a valid file for upload");

            ModalContent = ErrorDisplay.Create(error, null);

            return Page();
        }

        IFormFile? file = formData.Files[0];
        if (file is null || file.Length == 0)
        {
            var error = new Error("Page Upload", "You must select a valid file for upload");

            ModalContent = ErrorDisplay.Create(error, null);

            return Page();
        }

        try
        {
            if (file.ContentType != FileContentTypes.ExcelModernFile)
            {
                var error = new Error("Page Upload", "Only XLSX files are accepted");

                ModalContent = ErrorDisplay.Create(error, null);

                return Page();
            }

            await using MemoryStream target = new();
            await file.CopyToAsync(target);

            Result result = await _mediator.Send(new ImportProvisionsFromFileCommand(target));

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to upload Assessment Provisions by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(
                    result.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Compliance/AssessmentProvisions/Index", values: new { area = "Staff" }));

                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger
                .ForContext(nameof(Exception), ex, true)
                .Warning("Failed to upload External Reports by user {User}", _currentUserService.UserName);

            ModalContent = ExceptionDisplay.Create(
                ex,
                _linkGenerator.GetPathByPage("/StudentAdmin/Reports/External/Index", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxBulkDelete()
    {
        return Partial("ConfirmAssessmentProvisionBulkDeleteModal");
    }

    public async Task<IActionResult> OnGetBulkDelete()
    {
        _cacheService.Reset();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetQueueEmails()
    {
        Result result = await _mediator.Send(new SendNotificationsForAssessmentProvisionsCommand());

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to email Assessment Provision notifications by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Compliance/AssessmentProvisions/Index", values: new { area = "Staff"}));

            return Page();
        }

        return RedirectToPage();
    }
}