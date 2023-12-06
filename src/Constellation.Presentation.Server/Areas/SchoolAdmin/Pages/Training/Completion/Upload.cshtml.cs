namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training.Completion;

using Constellation.Application.Models.Auth;
using Constellation.Application.Training.Modules.ProcessTrainingImportFile;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Validation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
[RequestSizeLimit(10485760)]
public class UploadModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UploadModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty]
    [AllowExtensions(FileExtensions: "xlsx", ErrorMessage = "You can only upload XLSX files")]
    public IFormFile UploadFile { get; set; }

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Completions;
    [ViewData] public string StaffId { get; set; }

    public async Task OnGetAsync()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        if (UploadFile is not null)
        {
            try
            {
                await using MemoryStream target = new();
                await UploadFile.CopyToAsync(target);

                Result request = await _mediator.Send(new ProcessTrainingImportFileCommand(target));

                if (request.IsFailure)
                {
                    Error = new ErrorDisplay
                    {
                        Error = request.Error,
                        RedirectPath = _linkGenerator.GetPathByPage("/Training/Completion/Upload", values: new { area = "SchoolAdmin" })
                    };

                    return Page();
                }
            }
            catch (Exception ex)
            {
                Error = new ErrorDisplay
                {
                    Error = new(ex.Source, ex.Message),
                    RedirectPath = _linkGenerator.GetPathByPage("/Training/Completion/Upload", values: new { area = "SchoolAdmin" })
                };

                return Page();
            }
        }

        return RedirectToPage("Index");
    }
}
