namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Awards;

using Application.Common.PresentationModels;
using Application.Domains.Attachments.Queries.GetAttachmentFile;
using Application.Models.Auth;
using Constellation.Application.Domains.MeritAwards.Awards.Queries.GetSummaryForStudent;
using Constellation.Application.Domains.Students.Queries.GetStudentsByParentEmail;
using Constellation.Core.Models.Attachments.ValueObjects;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.Attachments.DTOs;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsParent)]
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
            .ForContext(LogDefaults.Application, LogDefaults.ParentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Awards;

    [BindProperty(SupportsGet = true)]
    public StudentId StudentId { get; set; } = StudentId.Empty;

    public StudentResponse? SelectedStudent { get; set; }

    public List<StudentResponse> Students { get; set; } = new();

    public StudentAwardSummaryResponse? AwardSummary { get; set; }

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnGetDownloadCertificate(StudentAwardId awardId)
    {
        Result<AttachmentResponse> fileResponse = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.AwardCertificate, awardId.ToString()));

        if (fileResponse.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                fileResponse.Error,
                _linkGenerator.GetPathByPage("/Awards/Index", values: new { area = "Parents" }));

            await PreparePage();

            return Page();
        }

        return File(fileResponse.Value.FileData, fileResponse.Value.FileType, fileResponse.Value.FileName);
    }

    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve student list by user {user}", _currentUserService.UserName);

        Result<List<StudentResponse>> studentsRequest = await _mediator.Send(new GetStudentsByParentEmailQuery(_currentUserService.EmailAddress));

        if (studentsRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(studentsRequest.Error);

            return;
        }

        Students = studentsRequest.Value
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        if (Students.Count == 1)
            StudentId = Students.First().StudentId;

        if (StudentId != StudentId.Empty)
        {
            _logger.Information("Requested to retrieve award data by user {user} for student {student}", _currentUserService.UserName, StudentId);
            
            Result<StudentAwardSummaryResponse> awardSummaryRequest = await _mediator.Send(new GetSummaryForStudentQuery(StudentId));

            if (awardSummaryRequest.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(awardSummaryRequest.Error);

                return;
            }

            AwardSummary = awardSummaryRequest.Value;
            SelectedStudent = Students.FirstOrDefault(entry => entry.StudentId == StudentId);
        }
    }
}