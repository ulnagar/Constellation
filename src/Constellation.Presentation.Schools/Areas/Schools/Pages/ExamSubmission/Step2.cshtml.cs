namespace Constellation.Presentation.Schools.Areas.Schools.Pages.ExamSubmission;

using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Courses.GetCoursesForStudent;
using Constellation.Application.Students.GetStudentsFromSchoolForSelectionList;
using Constellation.Core.Shared;
using Core.Models.Subjects.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class Step2Model : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public Step2Model(
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
    public CourseId CourseId { get; set; }

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
            nameof(StudentCourseResponse.DisplayName));

        return Page();
    }
}