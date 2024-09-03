namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Absences;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Students.Models;
using Constellation.Application.Students.GetCurrentStudentsFromSchool;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class ReportModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IDateTimeProvider _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ReportModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IDateTimeProvider dateTime,
        ICurrentUserService currentUserService,
        ILogger logger,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ReportModel>()
            .ForContext("APPLICATION", "Schools Portal");

        StartDate = dateTime.Today;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Absences;

    public List<StudentResponse> Students { get; set; } = new();

    [BindProperty]
    public List<StudentId> SelectedStudents { get; set; } = new();

    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly StartDate { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve student list by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);
        
        Result<List<StudentResponse>> studentsRequest = await _mediator.Send(new GetCurrentStudentsFromSchoolQuery(CurrentSchoolCode));

        if (studentsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                studentsRequest.Error,
                _linkGenerator.GetPathByPage("/Absences/Index", values: new { area = "Schools" }));

            return;
        }

        Students = studentsRequest.Value
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.Name.SortOrder)
            .ToList();
    }

    public async Task<IActionResult> OnPost()
    {
        // TODO: R1.16.0: Implement attendance report download for Schools Portal

        return Page();
    }

}