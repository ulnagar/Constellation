namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Reports.External;

using Application.Models.Auth;
using Constellation.Application.Attachments.GetTemporaryFiles;
using Constellation.Application.Attachments.ProcessPATReportZipFile;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Attachments.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;
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
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData]
    public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Reports_External;

    [ViewData]
    public string PageTitle => "External Report Upload";

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
            if (file.ContentType != MediaTypeNames.Application.Zip)
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
        AttachmentId attachmentId)
    {

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
