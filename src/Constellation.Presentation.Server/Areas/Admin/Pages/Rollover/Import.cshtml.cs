namespace Constellation.Presentation.Server.Areas.Admin.Pages.Rollover;

using Application.Common.PresentationModels;
using Application.DTOs;
using Application.Models.Auth;
using Application.Students.ImportStudentsFromFile;
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

    [BindProperty]
    public IFormFile FormFile { get; set; }

    public List<ImportStatusDto> Results { get; set; } = new();
    
    public async Task OnGet() { }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        if (FormFile is null)
        {
            ModalContent = new ErrorDisplay(new("Page.Form.File", "A file must be specified"));

            return Page();
        }

        try
        {
            // Verify that FormFile is xlsx/xls
            string[] allowedExtensions = { "xlsx", "xls" };
            if (!allowedExtensions.Contains(FormFile.FileName.Split('.').Last()))
            {
                ModalContent = new ErrorDisplay(new("Page.Form.File", "You must provide an Excel file (.xls or .xlsx)"));

                return Page();
            }

            await using MemoryStream target = new();
            await FormFile.CopyToAsync(target, cancellationToken);

            _logger.Information("Requested to import Students from file by user {User}", _currentUserService.UserName);

            Result<List<ImportStatusDto>> processRequest = await _mediator.Send(new ImportStudentsFromFileCommand(target), cancellationToken);

            if (processRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), processRequest.Error, true)
                    .Warning("Failed to import Students from file by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(processRequest.Error);

                return Page();
            }

            Results = processRequest.Value;
        }
        catch (Exception ex)
        {
            ModalContent = new ErrorDisplay(new("Exception", ex.Message));
        }
        
        return Page();
    }
}