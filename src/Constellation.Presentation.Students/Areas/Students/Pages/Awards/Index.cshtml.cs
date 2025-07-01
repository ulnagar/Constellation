namespace Constellation.Presentation.Students.Areas.Students.Pages.Awards;

using Application.Common.PresentationModels;
using Application.Domains.Attachments.Queries.GetAttachmentFile;
using Application.Domains.Students.Queries.GetStudentById;
using Application.Models.Auth;
using Constellation.Application.Domains.MeritAwards.Awards.Queries.GetSummaryForStudent;
using Constellation.Application.Domains.Students.Models;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Identifiers;
using Core.Models.Students.Errors;
using Core.Models.Students.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.IsStudent)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StudentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Awards;

    public StudentResponse Student { get; set; }

    public StudentAwardSummaryResponse? AwardSummary { get; set; }

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnGetDownloadCertificate(StudentAwardId awardId)
    {
        GetAttachmentFileQuery command = new(AttachmentType.AwardCertificate, awardId.ToString());

        _logger
            .ForContext(nameof(GetAttachmentFileQuery), command, true)
            .Information("Requested to retrieve award certificate by user {user}", _currentUserService.UserName);

        Result<AttachmentResponse> fileResponse = await _mediator.Send(command);

        if (fileResponse.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), fileResponse.Error, true)
                .Warning("Failed to retrieve award certificate by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                fileResponse.Error,
                _linkGenerator.GetPathByPage("/Awards/Index", values: new { area = "Students" }));

            await PreparePage();

            return Page();
        }

        return File(fileResponse.Value.FileData, fileResponse.Value.FileType, fileResponse.Value.FileName);
    }

    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve award summary by user {user}", _currentUserService.UserName);

        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to retrieve award summary by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(StudentErrors.InvalidId);

            return;
        }

        StudentId studentId = StudentId.FromValue(new (studentIdClaimValue));

        Result<StudentResponse> studentRequest = await _mediator.Send(new GetStudentByIdQuery(studentId));

        if (studentRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), studentRequest.Error, true)
                .Warning("Failed to retrieve award summary by user {user}", _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(studentRequest.Error);

            return;
        }

        Student = studentRequest.Value;
        
        Result<StudentAwardSummaryResponse> awardSummaryRequest = await _mediator.Send(new GetSummaryForStudentQuery(studentId));

        if (awardSummaryRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), awardSummaryRequest.Error, true)
                .Warning("Failed to retrieve award summary by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(awardSummaryRequest.Error);

            return;
        }

        AwardSummary = awardSummaryRequest.Value;
    }
}