namespace Constellation.Presentation.Schools.Areas.Schools.Pages.ExamSubmission;

using Application.Domains.Assignments.Commands.UploadAssignmentSubmission;
using Application.Domains.Assignments.Queries.GetAssignmentsByCourse;
using Application.DTOs;
using Application.Helpers;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Courses.Queries.GetCoursesForStudent;
using Constellation.Application.Domains.Students.Queries.GetStudentsFromSchoolForSelectionList;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.Assignments.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
[RequestSizeLimit(10485760)]
public class Step4Model : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public Step4Model(
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
            .ForContext<Step4Model>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Exams;

    public SelectList Students { get; set; }
    [BindProperty]
    public StudentId StudentId { get; set; }

    public SelectList Courses { get; set; }
    [BindProperty]
    public CourseId CourseId { get; set; }

    public SelectList Assignments { get; set; }
    [BindProperty]
    public AssignmentId AssignmentId { get; set; }

    [BindProperty]
    [AllowExtensions(FileExtensions: "pdf", ErrorMessage = "You can only upload PDF files")]
    public IFormFile? UploadFile { get; set; }

    public async Task<IActionResult> OnGet() => RedirectToPage("/ExamSubmission/Step1", new { area = "Schools" });

    public async Task<IActionResult> OnPost()
    {
        await PreparePage();

        return Page();
    }

    public async Task<IActionResult> OnPostSubmit()
    {
        // TODO: R1.18: Move these errors to a common location

        if (UploadFile is null)
        {
            ModalContent = ErrorDisplay.Create(
                new("FileEmpty", "You must select a file to upload"),
                _linkGenerator.GetPathByPage("/ExamSubmission/Step1", values: new { area = "Schools" }));

            return Page();
        }

        if (UploadFile.ContentType != FileContentTypes.PdfFile)
        {
            ModalContent = ErrorDisplay.Create(
                new ("FileTypeMismatch", "You can only upload PDF files"),
                _linkGenerator.GetPathByPage("/ExamSubmission/Step1", values: new { area = "Schools" }));

            return Page();
        }
        
        await using MemoryStream target = new();
        await UploadFile.CopyToAsync(target);

        FileDto file = new()
        {
            FileData = target.ToArray(),
            FileName = UploadFile.FileName,
            FileType = UploadFile.ContentType
        };
         
        UploadAssignmentSubmissionCommand command = new(
            AssignmentId,
            StudentId,
            file)
        {
            SubmittedBy = _currentUserService.UserName
        };

        _logger.Information("Requested to upload assignment submission by user {user} with file {file}", _currentUserService.UserName, file.FileName);
        
        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                request.Error,
                _linkGenerator.GetPathByPage("/ExamSubmission/Step1", values: new { area = "Schools" }));

            return Page();
        }

        ModalContent = FeedbackDisplay.Create(
            "Upload Successful",
            "The file has been uploaded successfully. You will receive an email receipt shortly.",
            "Ok",
            "btn-success",
            _linkGenerator.GetPathByPage("/ExamSubmission/Step1", values: new { area = "Schools" }));

        return Page();
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve student list by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);
        
        Result<List<StudentSelectionResponse>> studentsRequest = await _mediator.Send(new GetStudentsFromSchoolForSelectionQuery(CurrentSchoolCode));

        if (studentsRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                studentsRequest.Error,
                _linkGenerator.GetPathByPage("/ExamSubmission/Step1", values: new { area = "Schools" }));

            return;
        }

        List<StudentSelectionResponse> students = studentsRequest.Value
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        Students = new SelectList(students,
            nameof(StudentSelectionResponse.StudentId),
            nameof(StudentSelectionResponse.DisplayName),
            StudentId,
            nameof(StudentSelectionResponse.CurrentGrade));

        _logger.Information("Requested to retrieve course list by user {user} for student {student}", _currentUserService.UserName, StudentId);
        
        Result<List<StudentCourseResponse>> coursesRequest = await _mediator.Send(new GetCoursesForStudentQuery(StudentId));

        if (coursesRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                coursesRequest.Error,
                _linkGenerator.GetPathByPage("/ExamSubmission/Step1", values: new { area = "Schools" }));

            return;
        }

        List<StudentCourseResponse> courses = coursesRequest.Value
            .OrderBy(course => course.Name)
            .ToList();

        Courses = new SelectList(courses,
            nameof(StudentCourseResponse.Id),
            nameof(StudentCourseResponse.DisplayName),
            CourseId);

        _logger.Information("Requested to retrieve assignment list by user {user} for course {course}", _currentUserService.UserName, CourseId);

        Result<List<CourseAssignmentResponse>> assignmentsRequest = await _mediator.Send(new GetAssignmentsByCourseQuery(CourseId, StudentId));

        if (assignmentsRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                assignmentsRequest.Error,
                _linkGenerator.GetPathByPage("/ExamSubmission/Step1", values: new { area = "Schools" }));

            return;
        }

        List<CourseAssignmentResponse> assignments = assignmentsRequest.Value
            .OrderBy(assignment => assignment.DueDate)
            .ToList();

        Assignments = new SelectList(assignments,
            nameof(CourseAssignmentResponse.AssignmentId),
            nameof(CourseAssignmentResponse.DisplayName),
            AssignmentId);

        return;
    }
}