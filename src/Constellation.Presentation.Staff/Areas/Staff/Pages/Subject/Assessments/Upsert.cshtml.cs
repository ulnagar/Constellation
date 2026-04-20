namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Assessments;

using Application.Common.PresentationModels;
using Application.Domains.Assessments.Assessments.Commands.CreateAssessment;
using Application.Domains.Assessments.Assessments.Commands.UpdateAssessment;
using Application.Domains.Assessments.Assessments.Models;
using Application.Domains.Assessments.Assessments.Queries.GetAssessmentById;
using Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessments;
using Application.Domains.Courses.Models;
using Application.Domains.Courses.Queries.GetCoursesForSelectionList;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Abstractions.Services;
using Core.Models.Assessments.Identifiers;
using Core.Models.Subjects.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;

[HasPermission(AuthPermission.Subjects_Assessments_Edit_Value)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assessments_Assessments;
    [ViewData] public string PageTitle => "Edit Assessment";

    [BindProperty(SupportsGet = true)]
    public AssessmentId Id { get; set; } = AssessmentId.Empty;

    [BindProperty]
    public string Name { get; set; }
    
    [BindProperty]
    public CourseId CourseId { get; set; } = CourseId.Empty;

    [BindProperty]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTimeOffset DueDate { get; set; }

    [BindProperty]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTimeOffset AvailableFrom { get; set; }

    [BindProperty]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTimeOffset AvailableTo { get; set; }
    
    public SelectList CourseList { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (Id == AssessmentId.Empty)
        {
            DateTime dueDate = DateTime.Today.AddDays(7);
            TimeSpan dueTime = new(12, 0, 0);
            DueDate = new(dueDate + dueTime, DateTimeOffset.Now.Offset);
            AvailableFrom = DueDate.AddDays(-7);
            AvailableTo = DueDate.AddDays(7);

            return await PreparePage();
        }

        Result<AssessmentResponse> assessment = await _mediator.Send(new GetAssessmentByIdQuery(Id));

        if (assessment.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), assessment.Error, true)
                .Warning("Failed to retrieve Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                assessment.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Index", values: new { area = "Staff" }));

            return Page();
        }

        Name = assessment.Value.Name;
        CourseId = assessment.Value.CourseId;
        DueDate = assessment.Value.DueDate;
        AvailableFrom = assessment.Value.AvailableFrom;
        AvailableTo = assessment.Value.AvailableTo;

        return await PreparePage();
    }

    private async Task<IActionResult> PreparePage()
    {
        Result<List<CourseSelectListItemResponse>> courseList = await _mediator.Send(new GetCoursesForSelectionListQuery(true));

        if (courseList.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), courseList.Error, true)
                .Warning("Failed to retrieve Assessment by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                courseList.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Index", values: new { area = "Staff" }));

            return Page();
        }

        CourseList = new SelectList(courseList.Value, "Id", "DisplayName", CourseId == CourseId.Empty ? null : CourseId, "FacultyName");

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (Id == AssessmentId.Empty)
        {
            CreateAssessmentCommand command = new(Name, CourseId, DueDate, AvailableFrom, AvailableTo);

            _logger
                .ForContext(nameof(CreateAssessmentCommand), command, true)
                .Information("Attempting to create Assessment by user {User}", _currentUserService.UserName);

            Result<AssessmentId> result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(CreateAssessmentCommand), command, true)
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to create Assessment by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(result.Error);

                return await PreparePage();
            }

            return RedirectToPage("/Subject/Assessments/Details", new { area = "Staff", Id = result.Value });
        }
        else
        {
            UpdateAssessmentCommand command = new(Id, Name, CourseId, DueDate, AvailableFrom, AvailableTo);

            _logger
                .ForContext(nameof(UpdateAssessmentCommand), command, true)
                .Information("Attempting to update Assessment by user {User}", _currentUserService.UserName);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpdateAssessmentCommand), command, true)
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to update Assessment by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(result.Error);

                return await PreparePage();
            }

            return RedirectToPage("/Subject/Assessments/Details", new { area = "Staff", Id });
        }
    }
}