namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Awards;

using Application.Models.Auth;
using Constellation.Application.Attachments.GetAttachmentFile;
using Constellation.Application.Awards.GetSummaryForStudent;
using Constellation.Application.Students.GetStudentsFromSchoolForSelectionList;
using Constellation.Core.Errors;
using Constellation.Core.Models.Attachments.ValueObjects;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Models.Attachments.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Shared.Helpers.ModelBinders;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory)
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Awards;

    [BindProperty(SupportsGet = true)]
    public string StudentId { get; set; } = string.Empty;

    public List<StudentSelectionResponse> Students { get; set; } = new();

    public StudentAwardSummaryResponse? AwardSummary { get; set; }

    public async Task OnGet()
    {
        if (string.IsNullOrWhiteSpace(CurrentSchoolCode))
        {
            Error = new()
            {
                Error = ApplicationErrors.SchoolInvalid,
                RedirectPath = null
            };

            return;
        }

        await PreparePage();
    }

    public async Task<IActionResult> OnGetDownloadCertificate(
        [ModelBinder(typeof(StrongIdBinder))] StudentAwardId awardId)
    {
        Result<AttachmentResponse> fileResponse = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.AwardCertificate, awardId.ToString()));

        if (fileResponse.IsFailure)
        {
            Error = new()
            {
                Error = fileResponse.Error,
                RedirectPath = null
            };

            await PreparePage();

            return Page();
        }

        return File(fileResponse.Value.FileData, fileResponse.Value.FileType, fileResponse.Value.FileName);
    }

    public async Task PreparePage()
    {
        Result<List<StudentSelectionResponse>> studentsRequest = await _mediator.Send(new GetStudentsFromSchoolForSelectionQuery(CurrentSchoolCode));

        if (studentsRequest.IsFailure)
        {
            Error = new()
            {
                Error = studentsRequest.Error,
                RedirectPath = null
            };

            return;
        }

        Students = studentsRequest.Value
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        if (!string.IsNullOrWhiteSpace(StudentId))
        {
            Result<StudentAwardSummaryResponse> awardSummaryRequest = await _mediator.Send(new GetSummaryForStudentQuery(StudentId));

            if (awardSummaryRequest.IsFailure)
            {
                Error = new()
                {
                    Error = awardSummaryRequest.Error,
                    RedirectPath = null
                };

                return;
            }

            AwardSummary = awardSummaryRequest.Value;
        }
    }
}