namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Awards;

using Application.Common.PresentationModels;
using Application.Domains.Attachments.Queries.GetAttachmentFile;
using Application.Models.Auth;
using Constellation.Application.Domains.MeritAwards.Awards.Queries.GetSummaryForStudent;
using Constellation.Application.Domains.Students.Queries.GetStudentsFromSchoolForSelectionList;
using Constellation.Core.Errors;
using Constellation.Core.Models.Attachments.ValueObjects;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.Attachments.DTOs;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
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
        ILogger logger,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory)
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Awards;

    [BindProperty(SupportsGet = true)]
    public StudentId StudentId { get; set; } = StudentId.Empty;

    public List<StudentSelectionResponse> Students { get; set; } = new();

    public StudentAwardSummaryResponse? AwardSummary { get; set; }

    public async Task OnGet()
    {
        if (string.IsNullOrWhiteSpace(CurrentSchoolCode))
        {
            ModalContent = ErrorDisplay.Create(ApplicationErrors.SchoolInvalid);

            return;
        }

        await PreparePage();
    }

    public async Task<IActionResult> OnGetDownloadCertificate(StudentAwardId awardId)
    {
        Result<AttachmentResponse> fileResponse = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.AwardCertificate, awardId.ToString()));

        if (fileResponse.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(fileResponse.Error);

            await PreparePage();

            return Page();
        }

        return File(fileResponse.Value.FileData, fileResponse.Value.FileType, fileResponse.Value.FileName);
    }

    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve student list by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);

        Result<List<StudentSelectionResponse>> studentsRequest = await _mediator.Send(new GetStudentsFromSchoolForSelectionQuery(CurrentSchoolCode));

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
        }
    }
}