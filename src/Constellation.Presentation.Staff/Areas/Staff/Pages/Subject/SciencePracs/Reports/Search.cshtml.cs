namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Reports;

using Application.Common.PresentationModels;
using Application.SciencePracs.BulkCancelRolls;
using Constellation.Application.Models.Auth;
using Constellation.Application.Schools.GetSchoolById;
using Constellation.Application.SciencePracs.GetFilteredRollsForSchool;
using Constellation.Application.SciencePracs.GetFilteredRollsForStudent;
using Constellation.Application.SciencePracs.Models;
using Constellation.Application.Students.GetStudentById;
using Constellation.Application.Students.Models;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;
using Shared.Components.BulkCancelSciencePracRolls;

[Authorize(Policy = AuthPolicies.CanManageSciencePracs)]
public class SearchModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public SearchModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Reports;
    [ViewData] public string PageTitle => "Lesson Roll Reports";

    [BindProperty(SupportsGet = true)]
    public string SchoolCode { get; set; }

    [BindProperty(SupportsGet = true)]
    public StudentId StudentId { get; set; } = StudentId.Empty;

    public string ReportFor { get; set; }
    public List<RollSummaryResponse> Rolls { get; set; } = new();
    
    public async Task OnGet()
    {
        if (StudentId != StudentId.Empty)
        {
            _logger.Information("Requested to retrieve Lesson Rolls for Student with id {Id} by user {User}", StudentId, _currentUserService.UserName);

            Result<List<RollSummaryResponse>> request = await _mediator.Send(new GetFilteredRollsForStudentQuery(StudentId));

            if (request.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), request.Error, true)
                    .Warning("Failed to retrieve Lesson Rolls for Student with id {Id} by user {User}", StudentId, _currentUserService.UserName);
                
                ModalContent = new ErrorDisplay(request.Error);

                return;
            }

            Rolls = request.Value.OrderByDescending(entry => entry.DueDate).ToList();

            Result<StudentResponse> student = await _mediator.Send(new GetStudentByIdQuery(StudentId));

            ReportFor = student.IsSuccess ? student.Value.Name.DisplayName : $"student with Id {StudentId}";
        }

        if (!string.IsNullOrWhiteSpace(SchoolCode))
        {
            _logger.Information("Requested to retrieve Lesson Rolls for School with id {Id} by user {User}", SchoolCode, _currentUserService.UserName);

            Result<List<RollSummaryResponse>> request = await _mediator.Send(new GetFilteredRollsForSchoolQuery(SchoolCode));

            if (request.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), request.Error, true)
                    .Warning("Failed to retrieve Lesson Rolls for School with id {Id} by user {User}", SchoolCode, _currentUserService.UserName);

                ModalContent = new ErrorDisplay(request.Error);

                return;
            }

            Result<SchoolResponse> school = await _mediator.Send(new GetSchoolByIdQuery(SchoolCode));

            ReportFor = school.IsSuccess ? school.Value.Name : $"school with Code {SchoolCode}";

            Rolls = request.Value;
        }
    }
    
    public async Task<IActionResult> OnPostBulkCancel(BulkCancelSciencePracRollsSelection viewModel)
    {
        BulkCancelRollsCommand command = new(
            viewModel.SelectedSchoolCodes,
            viewModel.SelectedGrades,
            viewModel.Comment);

        _logger
            .ForContext(nameof(BulkCancelRollsCommand), command, true)
            .Information("Requested to bulk cancel Science Prac Rolls by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to bulk cancel Science Prac Rolls by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }

        return RedirectToPage();
    }
}