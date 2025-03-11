namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Completion;

using Application.Common.PresentationModels;
using Application.Training.ProcessTrainingImportFile;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Attributes;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
[RequestSizeLimit(10485760)]
public class UploadModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UploadModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UploadModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
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
                _logger.Information("Requested to upload certificate for Training Completion by user {User}", _currentUserService.UserName);

                await using MemoryStream target = new();
                await UploadFile.CopyToAsync(target);

                Result request = await _mediator.Send(new ProcessTrainingImportFileCommand(target));

                if (request.IsFailure)
                {
                    _logger
                        .ForContext(nameof(Error), request.Error, true)
                        .Warning("Failed to upload certificate for Training Completion by user {User}", _currentUserService.UserName);
                    
                    ModalContent = new ErrorDisplay(
                        request.Error,
                        _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Completion/Upload", values: new { area = "Staff" }));

                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger
                    .ForContext(nameof(Exception), ex, true)
                    .Warning("Failed to upload certificate for Training Completion by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    new(ex.Source, ex.Message),
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Completion/Upload", values: new { area = "Staff" }));

                return Page();
            }
        }

        return RedirectToPage("Index");
    }
}
