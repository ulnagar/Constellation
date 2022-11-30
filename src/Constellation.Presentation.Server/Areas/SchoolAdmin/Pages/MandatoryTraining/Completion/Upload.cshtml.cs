namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Completion;

using Constellation.Application.Features.MandatoryTraining.Commands;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Validation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
[RequestSizeLimit(10485760)]
public class UploadModel : BasePageModel
{
    private readonly IMediator _mediator;

    public UploadModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    [AllowExtensions(FileExtensions: "xlsx", ErrorMessage = "You can only upload XLSX files")]
    public IFormFile UploadFile { get; set; }

    public async Task OnGetAsync()
    {
        await GetClasses(_mediator);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (UploadFile is not null)
        {
            try
            {
                await using var target = new MemoryStream();
                await UploadFile.CopyToAsync(target);

                await _mediator.Send(new ProcessTrainingImportFileCommand(target));   
            }
            catch (Exception ex)
            {
                // Error uploading file
                throw;
            }
        }

        return RedirectToPage("Index");
    }
}
