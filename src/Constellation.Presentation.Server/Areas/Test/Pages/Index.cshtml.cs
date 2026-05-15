namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Domains.LinkedSystems.Canvas.Models;
using Application.DTOs;
using Application.Interfaces.Gateways;
using Application.Models.Auth;
using BaseModels;
using Core.Abstractions.Services;
using Core.Models.Canvas.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICanvasGateway _canvasGateway;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ICanvasGateway canvasGateway,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _canvasGateway = canvasGateway;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public List<CourseListEntry> Courses { get; set; }

    public async Task OnGet()
    {
        List<CourseListEntry> courses = await _canvasGateway.GetAllCourses("2026");

        Courses = courses;
    }

    public async Task<IActionResult> OnGetCourseAssignments(CanvasCourseCode course)
    {


        List<CanvasAssignmentDto> assignments = await _canvasGateway.GetAllCourseAssignments(course);
    }
}