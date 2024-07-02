namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Reports;

using Application.Common.PresentationModels;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Application.Schools.GetSchoolById;
using Constellation.Application.SciencePracs.GenerateOverdueReport;
using Constellation.Application.SciencePracs.GetFilteredRollsForSchool;
using Constellation.Application.SciencePracs.GetFilteredRollsForStudent;
using Constellation.Application.SciencePracs.Models;
using Constellation.Application.Students.GetStudentById;
using Constellation.Application.Students.Models;
using Constellation.Core.Shared;
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Reports;

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
                ModalContent = new ErrorDisplay(request.Error);

                return;
            }

            Rolls = request.Value.OrderByDescending(entry => entry.DueDate).ToList();

            Result<StudentResponse> student = await _mediator.Send(new GetStudentByIdQuery(StudentId));

            ReportFor = student.IsSuccess ? student.Value.Name.DisplayName : $"student with Id {StudentId}";

            return;
        }

        if (!string.IsNullOrWhiteSpace(SchoolCode))
        {
            Result<List<RollSummaryResponse>> request = await _mediator.Send(new GetFilteredRollsForSchoolQuery(SchoolCode));

            if (request.IsFailure)
            {
                ModalContent = new ErrorDisplay(request.Error);

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

        ModalContent = new ErrorDisplay(reportRequest.Error);

        return Page();
    }
}