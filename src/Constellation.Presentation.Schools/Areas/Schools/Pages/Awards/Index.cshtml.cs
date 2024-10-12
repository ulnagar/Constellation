namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Awards;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Constellation.Application.Attachments.GetAttachmentFile;
using Constellation.Application.Awards.GetSummaryForStudent;
using Constellation.Application.Students.GetStudentsFromSchoolForSelectionList;
using Constellation.Core.Errors;
using Constellation.Core.Models.Attachments.ValueObjects;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Attachments.DTOs;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Shared.Helpers.ModelBinders;
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
            .ForContext("APPLICATION", "Schools Portal");
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
            ModalContent = new ErrorDisplay(ApplicationErrors.SchoolInvalid);

            return;
        }

        await PreparePage();
    }

    public async Task<IActionResult> OnGetDownloadCertificate(StudentAwardId awardId)
    {
        Result<AttachmentResponse> fileResponse = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.AwardCertificate, awardId.ToString()));

        if (fileResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(fileResponse.Error);

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
            ModalContent = new ErrorDisplay(studentsRequest.Error);

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
                ModalContent = new ErrorDisplay(awardSummaryRequest.Error);

                return;
            }

            AwardSummary = awardSummaryRequest.Value;
        }
    }
}