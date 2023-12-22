namespace Constellation.Presentation.Server.Areas.Subject.Pages.SciencePracs.Reports;

using Application.DTOs;
using Application.Models.Auth;
using Application.Schools.GetSchoolById;
using Application.SciencePracs.GenerateOverdueReport;
using Application.SciencePracs.GetFilteredRollsForSchool;
using Application.SciencePracs.GetFilteredRollsForStudent;
using Application.SciencePracs.Models;
using Application.Students.GetStudentById;
using Application.Students.Models;
using BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanManageSciencePracs)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => SubjectPages.Reports;

    [BindProperty(SupportsGet = true)]
    public string SchoolCode { get; set; }

    [BindProperty(SupportsGet = true)]
    public string StudentId { get; set; }

    public string ReportFor { get; set; }
    public List<RollSummaryResponse> Rolls { get; set; } = new();
    
    public async Task OnGet()
    {
        if (!string.IsNullOrWhiteSpace(StudentId))
        {
            Result<List<RollSummaryResponse>> request = await _mediator.Send(new GetFilteredRollsForStudentQuery(StudentId));

            if (request.IsFailure)
            {
                Error = new()
                {
                    Error = request.Error,
                    RedirectPath = null
                };

                return;
            }

            Rolls = request.Value.OrderByDescending(entry => entry.DueDate).ToList();

            Result<StudentResponse> student = await _mediator.Send(new GetStudentByIdQuery(StudentId));

            ReportFor = student.IsSuccess ? student.Value.DisplayName : $"student with Id {StudentId}";

            return;
        }

        if (!string.IsNullOrWhiteSpace(SchoolCode))
        {
            Result<List<RollSummaryResponse>> request = await _mediator.Send(new GetFilteredRollsForSchoolQuery(SchoolCode));

            if (request.IsFailure)
            {
                Error = new()
                {
                    Error = request.Error,
                    RedirectPath = null
                };

                return;
            }

            Result<SchoolResponse> school = await _mediator.Send(new GetSchoolByIdQuery(SchoolCode));

            ReportFor = school.IsSuccess ? school.Value.Name : $"school with Code {SchoolCode}";

            Rolls = request.Value;
            return;
        }
    }

    public async Task<IActionResult> OnGetDownloadReport()
    {
        Result<FileDto> reportRequest = await _mediator.Send(new GenerateOverdueReportCommand());

        if (reportRequest.IsSuccess)
            return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);

        Error = new()
        {
            Error = reportRequest.Error,
            RedirectPath = null
        };

        return Page();
    }
}