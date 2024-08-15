namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Completion;

using Application.Common.PresentationModels;
using Application.Training.ProcessTrainingImportFile;
using Constellation.Application.Models.Auth;
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
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UploadModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Completions;
    [ViewData] public string PageTitle => "Upload Training Details";

    [BindProperty]
    [AllowExtensions(FileExtensions: "xlsx", ErrorMessage = "You can only upload XLSX files")]
    public IFormFile? UploadFile { get; set; }
    
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
                    ModalContent = new ErrorDisplay(
                        request.Error,
                        _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Completion/Upload", values: new { area = "Staff" }));

                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModalContent = new ErrorDisplay(
                    new(ex.Source, ex.Message),
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Completion/Upload", values: new { area = "Staff" }));

                return Page();
            }
        }

        return RedirectToPage("Index");
    }
}
