namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training.Completion;

using Constellation.Application.MandatoryTraining.Models;
using Constellation.Application.Models.Auth;
using Constellation.Application.Training.Modules.GetCompletionRecordDetails;
using Constellation.Application.Training.Modules.GetUploadedTrainingCertificateFileById;
using Constellation.Application.Training.Modules.MarkTrainingCompletionRecordDeleted;
using Constellation.Core.Models.Attachments.ValueObjects;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanViewTrainingCompletionRecord)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IAuthorizationService _authorizationService;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        IMediator mediator, 
        IAuthorizationService authorizationService,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid CompletionId { get; set; }
    [BindProperty(SupportsGet = true)]
    public Guid ModuleId { get; set; }

    public CompletionRecordDto Record { get; set; } = new();
    public CompletionRecordCertificateDetailsDto UploadedCertificate { get; set; } = new();

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Completions;
    [ViewData] public string StaffId { get; set; }

    public async Task OnGet()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        Result<CompletionRecordDto> recordRequest = await _mediator.Send(
            new GetCompletionRecordDetailsQuery(
                TrainingModuleId.FromValue(ModuleId),
                TrainingCompletionId.FromValue(CompletionId)));

        if (recordRequest.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = recordRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Training/Completion/Index", values: new { area = "SchoolAdmin" })
            };

            return;
        }

        Record = recordRequest.Value;

        Result<CompletionRecordCertificateDetailsDto> certificateRequest = await _mediator.Send(new GetUploadedTrainingCertificateFileByIdQuery(AttachmentType.TrainingCertificate, Record.Id.ToString()));

        if (certificateRequest.IsFailure)
            UploadedCertificate = null;
        else
            UploadedCertificate = certificateRequest.Value;
    }

    public async Task<IActionResult> OnGetDownloadCertificate()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        Result<CompletionRecordCertificateDetailsDto> certificateRequest = await _mediator.Send(new GetUploadedTrainingCertificateFileByIdQuery(AttachmentType.TrainingCertificate, Record.Id.ToString()));

        if (certificateRequest.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = certificateRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Training/Completion/Index", values: new { area = "SchoolAdmin" })
            };

            return Page();
        }

        UploadedCertificate = certificateRequest.Value;

        return File(UploadedCertificate.FileData, UploadedCertificate.FileType, UploadedCertificate.Name);
    }

    public async Task<IActionResult> OnPostDeleteRecord()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        AuthorizationResult canEditTest = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);
        
        if (canEditTest.Succeeded)
        {
            await _mediator.Send(new MarkTrainingCompletionRecordDeletedCommand(TrainingModuleId.FromValue(ModuleId), TrainingCompletionId.FromValue(CompletionId)));
        }

        return RedirectToPage("Index");
    }
}
