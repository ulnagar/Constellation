namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Completion;

using Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.Training.Modules.ProcessTrainingImportFile;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;

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

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Completions;
    
    public async Task OnGetAsync() {}

    public async Task<IActionResult> OnPostAsync()
    {
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
                        RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Completion/Upload", values: new { area = "Staff" })
                    };

                    return Page();
                }
            }
            catch (Exception ex)
            {
                Error = new ErrorDisplay
                {
                    Error = new(ex.Source, ex.Message),
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Completion/Upload", values: new { area = "Staff" })
                };

                return Page();
            }
        }

        return RedirectToPage("Index");
    }
}
