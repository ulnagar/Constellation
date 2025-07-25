namespace Constellation.Presentation.Server.Areas.Admin.Pages.Rollover;

using Application.Common.PresentationModels;
using Application.Domains.Students.Commands.ImportStudentsFromFile;
using Application.DTOs;
using Application.Models.Auth;
using BaseModels;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class ImportModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ImportModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ImportModel>();
    }
    [ViewData] public string ActivePage => Models.ActivePage.Rollover;
    [ViewData] public string PageTitle => "Annual Rollover";

    [BindProperty]
    public IFormFile FormFile { get; set; }

    [BindProperty]
    public bool RemoveExcess { get; set; }

    public List<ImportStatusDto> Results { get; set; } = new();
    
    public async Task OnGet() { }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        if (FormFile is null)
        {
            ModalContent = ErrorDisplay.Create(new("Page.Form.File", "A file must be specified"));

            return Page();
        }

        try
        {
            // TODO: R1.18: Move these manual errors to a common location

            // Verify that FormFile is xlsx/xls
            string[] allowedExtensions = { "xlsx", "xls" };
            if (!allowedExtensions.Contains(FormFile.FileName.Split('.').Last()))
            {
                ModalContent = ErrorDisplay.Create(new("Page.Form.File", "You must provide an Excel file (.xls or .xlsx)"));

                return Page();
            }

            await using MemoryStream target = new();
            await FormFile.CopyToAsync(target, cancellationToken);

            _logger.Information("Requested to import Students from file by user {User}", _currentUserService.UserName);

            Result<List<ImportStatusDto>> processRequest = await _mediator.Send(new ImportStudentsFromFileCommand(target, RemoveExcess), cancellationToken);

            if (processRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), processRequest.Error, true)
                    .Warning("Failed to import Students from file by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(processRequest.Error);

                return Page();
            }

            Results = processRequest.Value;
        }
        catch (Exception ex)
        {
            ModalContent = ExceptionDisplay.Create(ex);
        }
        
        return Page();
    }
}