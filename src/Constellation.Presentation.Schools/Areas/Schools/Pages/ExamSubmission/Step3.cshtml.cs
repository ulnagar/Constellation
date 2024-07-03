namespace Constellation.Presentation.Schools.Areas.Schools.Pages.ExamSubmission;

using Application.Models.Auth;
using Constellation.Application.Assignments.GetAssignmentsByCourse;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Courses.GetCoursesForStudent;
using Constellation.Application.Students.GetStudentsFromSchoolForSelectionList;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Core.Models.Assignments.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Shared.Helpers.ModelBinders;
using static System.Net.WebRequestMethods;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class Step3Model : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public Step3Model(
        ISender mediator,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
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
    public AssignmentId AssignmentId { get; set; }

    public async Task<IActionResult> OnGet() => RedirectToPage("/ExamSubmission/Step1", new { area = "Schools" });

    public async Task<IActionResult> OnPost()
    {
        Result<List<StudentSelectionResponse>> studentsRequest = await _mediator.Send(new GetStudentsFromSchoolForSelectionQuery(CurrentSchoolCode));

        if (studentsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                studentsRequest.Error,
                _linkGenerator.GetPathByPage("/ExamSubmission/Step1", values: new { area = "Schools" }));
            
            return Page();
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

            return Page();
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

            return Page();
        }

        List<CourseAssignmentResponse> assignments = assignmentsRequest.Value
            .OrderBy(assignment => assignment.DueDate)
            .ToList();

        Assignments = new SelectList(assignments,
            nameof(CourseAssignmentResponse.AssignmentId),
            nameof(CourseAssignmentResponse.DisplayName));

        return Page();
    }
}