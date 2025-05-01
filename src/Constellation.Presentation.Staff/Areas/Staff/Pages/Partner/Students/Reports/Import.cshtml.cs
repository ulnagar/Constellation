namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Reports;

using Application.Domains.Students.Commands.ImportStudentsFromFile;
using Application.DTOs;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class ImportModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ImportModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ImportModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Reports;
    [ViewData] public string PageTitle => "Student Import";

    [BindProperty]
    [AllowExtensions(FileExtensions: "xlsx", ErrorMessage = "You can only upload XLSX files")]
    public IFormFile? UploadFile { get; set; }

    [BindProperty]
    public bool RemoveExcess { get; set; }

    public List<ImportStatusDto> Statuses { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostImportFile()
    {
        if (UploadFile is null)
        {
            ModelState.AddModelError("UploadFile", "You must select a file to upload and process");

            return Page();
        }

        try
        {
            await using MemoryStream target = new();
            await UploadFile.CopyToAsync(target);

            _logger
                .Information("Requested to import Assets from file by user {User}", _currentUserService.UserName);

            Result<List<ImportStatusDto>> request = await _mediator.Send(new ImportStudentsFromFileCommand(target, RemoveExcess));

            if (request.IsFailure)
            {
                ModalContent = new ErrorDisplay(request.Error);

                _logger
                    .ForContext(nameof(Error), request.Error, true)
                    .Warning("Failed to import Students from file by user {User}", _currentUserService.UserName);

                return Page();
            }

            if (request.Value.Count > 0)
            {
                Statuses = request.Value;

                return Page();
            }
        }
        catch (Exception ex)
        {
            ModalContent = new ErrorDisplay(new(ex.Source, ex.Message));

            _logger
                .ForContext(nameof(Exception), ex, true)
                .Warning("Failed to import Students from file by user {User}", _currentUserService.UserName);

            return Page();
        }

        return RedirectToPage("/Partner/Students/Index", new { area = "Staff" });
    }
}