namespace Constellation.Presentation.Server.Areas.Admin.Pages.Rollover;

using Application.Common.PresentationModels;
using Application.Interfaces.Services;
using Application.Models.Auth;
using Application.Rollover.ImportStudents;
using BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class ImportModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IExcelService _excelService;

    public ImportModel(
        ISender mediator,
        IExcelService excelService)
    {
        _mediator = mediator;
        _excelService = excelService;
    }

    [BindProperty]
    public IFormFile FormFile { get; set; }

    public List<ImportResult> Results { get; set; } = new();
    
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

            Result<List<StudentImportRecord>> outputRequest = await _excelService.ConvertStudentImportFile(target, cancellationToken);

            if (outputRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(outputRequest.Error);

                return Page();
            }

            Result<List<ImportResult>> processRequest = await _mediator.Send(new ImportStudentsCommand(outputRequest.Value), cancellationToken);

            if (processRequest.IsFailure)
            {
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