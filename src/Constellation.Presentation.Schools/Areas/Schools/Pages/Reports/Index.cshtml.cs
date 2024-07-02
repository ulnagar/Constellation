namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Reports;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Reports.GetStudentReportsForSchool;
using Constellation.Application.Attachments.GetAttachmentFile;
using Constellation.Core.Models.Attachments.DTOs;
using Constellation.Core.Models.Attachments.ValueObjects;
using Core.Models.Identifiers;
using Core.Shared;
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

    [ViewData] public string ActivePage => Models.ActivePage.Reports;

    public List<SchoolStudentReportResponse> Reports { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<SchoolStudentReportResponse>> reportsResponse = await _mediator.Send(new GetStudentReportsForSchoolQuery(CurrentSchoolCode));
        
        if (reportsResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(reportsResponse.Error);

            return;
        }

        Reports = reportsResponse.Value
            .OrderBy(report => report.Grade)
            .ThenBy(report => report.LastName)
            .ThenBy(report => report.FirstName)
            .ToList();
    }

    public async Task<IActionResult> OnGetDownload(
        [ModelBinder(typeof(StrongIdBinder))] AcademicReportId reportId)
    {
        Result<AttachmentResponse> file = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.StudentReport, reportId.ToString()));
        
        if (file.IsFailure)
        {
            ModalContent = new ErrorDisplay(file.Error);

            Result<List<SchoolStudentReportResponse>> reportsResponse = await _mediator.Send(new GetStudentReportsForSchoolQuery(CurrentSchoolCode));

            Reports = reportsResponse.Value
                .OrderBy(report => report.Grade)
                .ThenBy(report => report.LastName)
                .ThenBy(report => report.FirstName)
                .ToList();

            return Page();
        }

        return File(file.Value.FileData, file.Value.FileType, file.Value.FileName);
    }
}