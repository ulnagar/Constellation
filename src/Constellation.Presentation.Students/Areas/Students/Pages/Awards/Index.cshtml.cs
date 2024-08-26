﻿namespace Constellation.Presentation.Students.Areas.Students.Pages.Awards;

using Application.Attachments.GetAttachmentFile;
using Application.Awards.GetSummaryForStudent;
using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Students.GetStudentById;
using Application.Students.Models;
using Core.Abstractions.Services;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Identifiers;
using Core.Models.Students.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
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
            .ForContext(StudentLogDefaults.Application, StudentLogDefaults.StudentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Awards;

    public StudentResponse Student { get; set; }

    public StudentAwardSummaryResponse? AwardSummary { get; set; }

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnGetDownloadCertificate(
        [ModelBinder(typeof(ConstructorBinder))] StudentAwardId awardId)
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

            ModalContent = new ErrorDisplay(
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

        string studentId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentId))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to retrieve award summary by user {user}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(StudentErrors.InvalidId);

            return;
        }

        Result<StudentResponse> studentRequest = await _mediator.Send(new GetStudentByIdQuery(studentId));

        if (studentRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), studentRequest.Error, true)
                .Warning("Failed to retrieve award summary by user {user}", _currentUserService.UserName);
            
            ModalContent = new ErrorDisplay(studentRequest.Error);

            return;
        }

        Student = studentRequest.Value;
        
        Result<StudentAwardSummaryResponse> awardSummaryRequest = await _mediator.Send(new GetSummaryForStudentQuery(studentId));

        if (awardSummaryRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), awardSummaryRequest.Error, true)
                .Warning("Failed to retrieve award summary by user {user}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(awardSummaryRequest.Error);

            return;
        }

        AwardSummary = awardSummaryRequest.Value;
    }
}