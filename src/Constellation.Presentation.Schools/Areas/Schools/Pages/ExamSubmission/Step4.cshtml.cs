namespace Constellation.Presentation.Schools.Areas.Schools.Pages.ExamSubmission;

using Application.Assignments.UploadAssignmentSubmission;
using Application.DTOs;
using Application.Helpers;
using Application.Models.Auth;
using Constellation.Application.Assignments.GetAssignmentsByCourse;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Courses.GetCoursesForStudent;
using Constellation.Application.Students.GetStudentsFromSchoolForSelectionList;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Core.Abstractions.Services;
using Core.Models.Assignments.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
[RequestSizeLimit(10485760)]
public class Step4Model : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;

    public Step4Model(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Exams;

    public SelectList Students { get; set; }
    [BindProperty]
    public string StudentId { get; set; }

    public SelectList Courses { get; set; }
    [BindProperty]
    [ModelBinder(typeof(StrongIdBinder))]
    public CourseId CourseId { get; set; }

    public SelectList Assignments { get; set; }
    [BindProperty]
    [ModelBinder(typeof(StrongIdBinder))]
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
        if (UploadFile is null)
        {
            ModalContent = new ErrorDisplay(
                new("FileEmpty", "You must select a file to upload"),
                _linkGenerator.GetPathByPage("/ExamSubmission/Step1", values: new { area = "Schools" }));

            return Page();
        }

        if (UploadFile.ContentType != FileContentTypes.PdfFile)
        {
            ModalContent = new ErrorDisplay(
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

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/ExamSubmission/Step1", values: new { area = "Schools" }));

            return Page();
        }

        ModalContent = new FeedbackDisplay(
            "Upload Successful",
            "The file has been uploaded successfully. You will receive an email receipt shortly.",
            "Ok",
            "btn-success",
            _linkGenerator.GetPathByPage("/ExamSubmission/Step1", values: new { area = "Schools" }));

        return Page();
    }

    private async Task PreparePage()
    {
        Result<List<StudentSelectionResponse>> studentsRequest = await _mediator.Send(new GetStudentsFromSchoolForSelectionQuery(CurrentSchoolCode));

        if (studentsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
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

        Result<List<StudentCourseResponse>> coursesRequest = await _mediator.Send(new GetCoursesForStudentQuery(StudentId));

        if (coursesRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
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

        Result<List<CourseAssignmentResponse>> assignmentsRequest = await _mediator.Send(new GetAssignmentsByCourseQuery(CourseId, StudentId));

        if (assignmentsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
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