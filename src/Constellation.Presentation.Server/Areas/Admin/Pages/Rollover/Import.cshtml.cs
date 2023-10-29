namespace Constellation.Presentation.Server.Areas.Admin.Pages.Rollover;

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
    
    public async Task OnGet()
    {
        await GetClasses(_mediator);
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        if (FormFile is null)
        {

            return Page();
        }

        try
        {
            await using MemoryStream target = new();
            await FormFile.CopyToAsync(target, cancellationToken);

            Result<List<StudentImportRecord>> outputRequest = await _excelService.ConvertStudentImportFile(target, cancellationToken);

            if (outputRequest.IsFailure)
            {
                Error = new()
                {
                    Error = outputRequest.Error,
                    RedirectPath = null
                };

                return Page();
            }

            Result<List<ImportResult>> processRequest = await _mediator.Send(new ImportStudentsCommand(outputRequest.Value), cancellationToken);

            if (processRequest.IsFailure)
            {
                Error = new()
                {
                    Error = processRequest.Error,
                    RedirectPath = null
                };

                return Page();
            }

            Results = processRequest.Value;
        }
        catch (Exception ex)
        {
            Error = new()
            {
                Error = new("Exception", ex.Message),
                RedirectPath = null
            };
        }
        
        return Page();
    }
}