namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Completion;

using Constellation.Application.Features.MandatoryTraining.Commands;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models;
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

    public DetailsModel(IMediator mediator, IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }
    public CompletionRecordDto Record { get; set; }
    public CompletionRecordCertificateDetailsDto UploadedCertificate { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Record = await _mediator.Send(new GetCompletionRecordDetailsQuery { Id = Id });

        UploadedCertificate = await _mediator.Send(new GetUploadedTrainingCertificateFileByIdQuery { LinkType = StoredFile.TrainingCertificate, LinkId = Record.Id.ToString() });

        //TODO: Check if return value is null, redirect and display error
    }

    public async Task<IActionResult> OnGetDownloadCertificate()
    {
        await GetClasses(_mediator);
        UploadedCertificate = await _mediator.Send(new GetUploadedTrainingCertificateFileByIdQuery { LinkType = StoredFile.TrainingCertificate, LinkId = Id.ToString() });

        return File(UploadedCertificate.FileData, UploadedCertificate.FileType, UploadedCertificate.Name);
    }

    public async Task<IActionResult> OnPostDeleteRecord()
    {
        var canEditTest = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);
        
        if (canEditTest.Succeeded)
        {
            await _mediator.Send(new MarkTrainingCompletionRecordDeletedCommand(Id));
        }

        return RedirectToPage("Index");
    }
}
