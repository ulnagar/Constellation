namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Completion;

using Application.Common.PresentationModels;
using Application.Domains.Training.Commands.MarkTrainingCompletionRecordDeleted;
using Application.Domains.Training.Models;
using Application.Domains.Training.Queries.GetCompletionRecordDetails;
using Application.Domains.Training.Queries.GetUploadedTrainingCertificateFileById;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Attachments.ValueObjects;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanViewTrainingCompletionRecord)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authorizationService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator, 
        IAuthorizationService authorizationService,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Completions;
    [ViewData] public string PageTitle => Record is not null ? $"Training Completion {Record.ModuleName}" : "Training Completion Details";


    [BindProperty(SupportsGet = true)]
    public TrainingCompletionId CompletionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public TrainingModuleId ModuleId { get; set; }

    public CompletionRecordDto? Record { get; set; } = new();
    public CompletionRecordCertificateDetailsDto? UploadedCertificate { get; set; } = new();


    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnGetDownloadCertificate()
    {
        _logger.Information("Requested to retrieve certificate for Training Completion by user {User}", _currentUserService.UserName);

        Result<CompletionRecordCertificateDetailsDto> certificateRequest = await _mediator.Send(new GetUploadedTrainingCertificateFileByIdQuery(AttachmentType.TrainingCertificate, Record.Id.ToString()));

        if (certificateRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), certificateRequest.Error, true)
                .Warning("Failed to retrieve certificate for Training Completion by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                certificateRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Completion/Index", values: new { area = "Staff" }));

            return Page();
        }

        UploadedCertificate = certificateRequest.Value;

        return File(UploadedCertificate.FileData, UploadedCertificate.FileType, UploadedCertificate.Name);
    }

    public async Task<IActionResult> OnGetDeleteRecord()
    {
        AuthorizationResult canEditTest = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!canEditTest.Succeeded) return RedirectToPage("/SchoolAdmin/Training/Completion/Index", new { area = "Staff" });

        MarkTrainingCompletionRecordDeletedCommand command = new(ModuleId, CompletionId);

        _logger
            .ForContext(nameof(MarkTrainingCompletionRecordDeletedCommand), command, true)
            .Information("Requested to delete Training Completion by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to delete Training Completion by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Training/Completion/Index", new { area = "Staff" });
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve details of Training Completion by user {User}", _currentUserService.UserName);

        Result<CompletionRecordDto> recordRequest = await _mediator.Send(
            new GetCompletionRecordDetailsQuery(
                ModuleId,
                CompletionId));

        if (recordRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), recordRequest.Error, true)
                .Warning("Failed to retrieve details of Training Completion by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                recordRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Completion/Index", values: new { area = "Staff" }));

            return;
        }

        Record = recordRequest.Value;

        Result<CompletionRecordCertificateDetailsDto> certificateRequest = await _mediator.Send(new GetUploadedTrainingCertificateFileByIdQuery(AttachmentType.TrainingCertificate, Record.Id.ToString()));

        if (certificateRequest.IsFailure)
            UploadedCertificate = null;
        else
            UploadedCertificate = certificateRequest.Value;
    }
}
