namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Reports.External;

using Application.Domains.Attachments.Commands.BulkPublishTemporaryFiles;
using Application.Domains.Attachments.Commands.DeleteTemporaryFile;
using Application.Domains.Attachments.Commands.EmailExternalReports;
using Application.Domains.Attachments.Commands.ProcessPATReportZipFile;
using Application.Domains.Attachments.Commands.PublishTemporaryFile;
using Application.Domains.Attachments.Models;
using Application.Domains.Attachments.Queries.GetTemporaryFileById;
using Application.Domains.Attachments.Queries.GetTemporaryFiles;
using Application.Domains.StudentReports.Commands.UpdateTempReportDetails;
using Application.Domains.Students.Queries.GetCurrentStudentsAsDictionary;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Reports.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using Shared.Components.EmailExternalReports;
using Shared.PartialViews.ConfirmTempReportDeleteModal;
using Shared.PartialViews.ConfirmTempReportPublishModal;
using Shared.PartialViews.UpdateTempReportDetails;
using System.Net.Mime;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanManageReports)]
public class BulkUploadModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public BulkUploadModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<BulkUploadModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Reports_External;
    [ViewData] public string PageTitle => "External Report Upload";

    public List<ExternalReportTemporaryFileResponse> Files { get; set; } = new();
    public List<string> Messages { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task OnPost([FromForm] IFormCollection formData)
    {
        if (formData is null || formData.Files.Count == 0)
        {
            var error = new Error("Page Upload", "You must select a valid file for upload");

            ModalContent = new ErrorDisplay(error, null);

            return;
        }

        IFormFile? file = formData.Files[0];
        if (file is null || file.Length == 0)
        {
            var error = new Error("Page Upload", "You must select a valid file for upload");

            ModalContent = new ErrorDisplay(error, null);

            return;
        }

        try
        {
            if (file.ContentType != MediaTypeNames.Application.Zip &&
                file.ContentType != "application/x-zip-compressed")
            {
                var error = new Error("Page Upload", "Only ZIP files are accepted");

                ModalContent = new ErrorDisplay(error, null);

                return;
            }
            
            await using MemoryStream target = new();
            await file.CopyToAsync(target);

            Result<List<string>> result = await _mediator.Send(new ProcessPATReportZipFileCommand(target));

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to upload External Reports by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    result.Error,
                    _linkGenerator.GetPathByPage("/StudentAdmin/Reports/External/Index", values: new { area = "Staff" }));

                return;
            }

            Messages = result.Value;
        }
        catch (Exception ex)
        {
            _logger
                .ForContext(nameof(Exception), ex, true)
                .Warning("Failed to upload External Reports by user {User}", _currentUserService.UserName);

            ModalContent = new ExceptionDisplay(
                ex,
                _linkGenerator.GetPathByPage("/StudentAdmin/Reports/External/Index", values: new { area = "Staff" }));

            return;
        }

        await PreparePage();
    }

    public async Task<IActionResult> OnPostAjaxUpdate(
        ExternalReportId reportId)
    {
        Result<ExternalReportTemporaryFileResponse> report = await _mediator.Send(new GetTemporaryFileByIdQuery(reportId));

        if (report.IsFailure)
            return Content(string.Empty);

        UpdateTempReportDetailsViewModel viewModel = new();

        viewModel.ReportId = reportId;
        viewModel.StudentId = report.Value.StudentId;
        viewModel.FileName = report.Value.FileName;
        viewModel.ReportType = report.Value.ReportType;
        viewModel.IssuedDate = report.Value.IssuedDate;

        Result<Dictionary<StudentId, string>> students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        if (students.IsFailure)
            return Content(string.Empty);

        viewModel.Students = new SelectList(students.Value, "Key", "Value", report.Value.StudentId);
        
        return Partial("UpdateTempReportDetails", viewModel);
    }

    public async Task<IActionResult> OnPostUpdateReport(
        UpdateTempReportDetailsViewModel viewModel)
    {
        UpdateTempReportDetailsCommand command = new(
            viewModel.ReportId,
            viewModel.StudentId,
            viewModel.ReportType,
            viewModel.IssuedDate);

        _logger
            .ForContext(nameof(UpdateTempReportDetailsCommand), command, true)
            .Information("Requested to update Temporary External Report by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update Temporary External Report by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Reports/External/BulkUpload", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxEmailReports()
    {
        return Partial("ConfirmTempReportEmailModal");
    }

    public async Task<IActionResult> OnPostEmailReports(
        EmailExternalReportsSelection viewModel)
    {
        await _mediator.Send(new EmailExternalReportsCommand(viewModel.Subject, viewModel.Body));

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxBulkPublish()
    {
        return Partial("ConfirmTempReportBulkPublishModal");
    }

    public async Task<IActionResult> OnGetBulkPublish()
    {
        _logger.Information("Requested to bulk publish temporary files by user {User}", _currentUserService.UserName);

        Result attempt = await _mediator.Send(new BulkPublishTemporaryFilesCommand());
        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to bulk publish temporary file by user {User}", _currentUserService.UserName);


            ModalContent = new ErrorDisplay(
                attempt.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Reports/External/BulkUpload", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxPublish(
        ExternalReportId reportId,
        string fileName)
    {
        ConfirmTempReportPublishModalViewModel viewModel = new(
            reportId,
            fileName);

        return Partial("ConfirmTempReportPublishModal", viewModel);
    }

    public async Task<IActionResult> OnGetPublishReport(
        ExternalReportId reportId)
    {
        PublishTemporaryFileCommand command = new(reportId);

        _logger
            .ForContext(nameof(PublishTemporaryFileCommand), command, true)
            .Information("Requested to publish temporary file by user {User}", _currentUserService.UserName);

        Result attempt = await _mediator.Send(command);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(PublishTemporaryFileCommand), command, true)
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to publish temporary file by user {User}", _currentUserService.UserName);


            ModalContent = new ErrorDisplay(
                attempt.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Reports/External/BulkUpload", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxDelete(
        ExternalReportId reportId,
        string fileName)
    {
        ConfirmTempReportDeleteModalViewModel viewModel = new(
            reportId,
            fileName);

        return Partial("ConfirmTempReportDeleteModal", viewModel);
    }

    public async Task<IActionResult> OnGetDeleteReport(
        ExternalReportId reportId)
    {
        DeleteTemporaryFileCommand command = new(reportId);

        _logger
            .ForContext(nameof(DeleteTemporaryFileCommand), command, true)
            .Information("Requested to delete temporary file by user {User}", _currentUserService.UserName);

        Result attempt = await _mediator.Send(command);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(DeleteTemporaryFileCommand), command, true)
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to delete temporary file by user {User}", _currentUserService.UserName);


            ModalContent = new ErrorDisplay(
                attempt.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Reports/External/BulkUpload", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage();
    }

    private async Task PreparePage()
    {
        Result<List<ExternalReportTemporaryFileResponse>> existingFiles = await _mediator.Send(new GetTemporaryFilesQuery());

        if (existingFiles.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), existingFiles.Error, true)
                .Warning("Failed to retrieve External Reports by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                existingFiles.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Reports/External/Index", values: new { area = "Staff" }));

            return;
        }

        Files = existingFiles.Value;
    }
}
