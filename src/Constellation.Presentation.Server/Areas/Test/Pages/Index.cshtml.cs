namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.DTOs.Canvas;
using Application.Interfaces.Gateways;
using BaseModels;
using Core.Models.Canvas.Models;

public class IndexModel : BasePageModel
{
    private readonly ICanvasGateway _gateway;


    public IndexModel(
        ICanvasGateway gateway)
    {
        _gateway = gateway;
    }

    public RubricEntry Rubric { get; set; }
    public List<CourseEnrolmentEntry> Enrolments { get; set; } = new();
    public List<AssignmentResultEntry> Submissions { get; set; } = new();
    
    public async Task OnGet()
    {
    }

    public async Task OnGetGetAssignment()
    {
        CanvasCourseCode courseCode = CanvasCourseCode.FromValue("2024-08ENG");
        int assignmentId = 1154;

        Rubric = await _gateway.GetCourseAssignmentDetails(courseCode, assignmentId, default);

        Enrolments = await _gateway.GetEnrolmentsForCourse(courseCode, default);

        Submissions = await _gateway.GetCourseAssignmentSubmissions(courseCode, assignmentId, default);
        
    }
}