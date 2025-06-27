namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Stocktake.Sighting;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.AssetManagement.Stocktake.Commands.RegisterManualSighting;
using Constellation.Application.Domains.Schools.Queries.GetSchoolById;
using Constellation.Application.Domains.StaffMembers.Models;
using Constellation.Application.Domains.StaffMembers.Queries.GetStaffFromSchool;
using Constellation.Application.Domains.Students.Models;
using Constellation.Application.Domains.Students.Queries.GetCurrentStudentsFromSchool;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Stocktake.Enums;
using Constellation.Core.Models.Stocktake.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class ManualModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public ManualModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        ILogger logger,
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory serviceFactory)
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _logger = logger
            .ForContext<ManualModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Stocktake;

    [BindProperty(SupportsGet = true)]
    public StocktakeEventId Id { get; set; }
    [BindProperty]
    public string SerialNumber { get; set; }
    [BindProperty]
    public string Description { get; set; }

    [BindProperty]
    [ModelBinder(typeof(BaseFromValueBinder))]
    public UserType UserType { get; set; }
    [BindProperty]
    public string UserName { get; set; }
    [BindProperty]
    public string UserCode { get; set; }
    [BindProperty]
    public string? Comment { get; set; }

    public List<StudentResponse> StudentList { get; set; } = new();
    public List<StaffSelectionListResponse> StaffList { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost()
    {
        Result<SchoolResponse> school = await _mediator.Send(new GetSchoolByIdQuery(CurrentSchoolCode));

        if (UserType.Equals(UserType.School))
        {
            UserName = school.Value.Name;
            UserCode = school.Value.SchoolCode;
        }

        RegisterManualSightingCommand command = new(
            Id,
            SerialNumber,
            Description,
            LocationCategory.PublicSchool,
            school.Value.Name,
            school.Value.SchoolCode,
            UserType,
            UserName,
            UserCode,
            Comment);

        _logger
            .ForContext(nameof(RegisterManualSightingCommand), command, true)
            .Information("Requested to submit stocktake sighting by user {user}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            await PreparePage();

            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }

        return RedirectToPage("/Stocktake/Index", new { area = "Schools", Id });
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve student list by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);

        Result<List<StudentResponse>> students = await _mediator.Send(new GetCurrentStudentsFromSchoolQuery(CurrentSchoolCode));

        if (students.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                students.Error,
                _linkGenerator.GetPathByPage("/Stocktake/Index", values: new { area = "Schools", Id }));

            return;
        }

        StudentList = students.Value
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.Name.SortOrder)
            .ToList();

        _logger.Information("Requested to retrieve staff list by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);


        Result<List<StaffSelectionListResponse>> teachers = await _mediator.Send(new GetStaffFromSchoolQuery(CurrentSchoolCode));

        if (teachers.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                teachers.Error,
                _linkGenerator.GetPathByPage("/Stocktake/Index", values: new { area = "Schools", Id }));

            return;
        }

        StaffList = teachers.Value
            .OrderBy(teacher => teacher.Name.SortOrder)
            .ToList();
    }
}