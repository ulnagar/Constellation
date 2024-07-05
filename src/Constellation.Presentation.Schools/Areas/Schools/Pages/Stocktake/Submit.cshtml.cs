namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Stocktake;

using Application.Models.Auth;
using Application.Schools.GetSchoolById;
using Application.StaffMembers.GetStaffFromSchool;
using Application.Stocktake.RegisterSighting;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.DTOs;
using Constellation.Application.StaffMembers.Models;
using Constellation.Application.Students.GetCurrentStudentsFromSchool;
using Constellation.Core.Models.Stocktake;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class SubmitModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public SubmitModel(
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
            .ForContext<SubmitModel>()
            .ForContext("APPLICATION", "Schools Portal");
    }

    [ViewData] public string ActivePage => Models.ActivePage.Stocktake;

    [BindProperty(SupportsGet = true)]
    public Guid EventId { get; set; }

    [BindProperty] public string? SerialNumber { get; set; } = string.Empty;
    [BindProperty] public string? AssetNumber { get; set; } = string.Empty;
    [BindProperty] public string Description { get; set; } = string.Empty;
    [BindProperty] public string UserType { get; set; } = string.Empty;
    [BindProperty] public string UserName { get; set; } = string.Empty;
    [BindProperty] public string UserCode { get; set; } = string.Empty;
    [BindProperty] public string? Comment { get; set; } = string.Empty;

    public List<StudentDto> StudentList { get; set; } = new();
    public List<StaffSelectionListResponse> StaffList { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve student list by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);

        Result<List<StudentDto>> students = await _mediator.Send(new GetCurrentStudentsFromSchoolQuery(CurrentSchoolCode));

        if (students.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                students.Error,
                _linkGenerator.GetPathByPage("/Stocktake/Index", values: new { area = "Schools", EventId }));

            return;
        }

        StudentList = students.Value
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        _logger.Information("Requested to retrieve staff list by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);


        Result<List<StaffSelectionListResponse>> teachers = await _mediator.Send(new GetStaffFromSchoolQuery(CurrentSchoolCode));

        if (teachers.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                teachers.Error,
                _linkGenerator.GetPathByPage("/Stocktake/Index", values: new { area = "Schools", EventId }));

            return;
        }

        StaffList = teachers.Value
            .OrderBy(teacher => teacher.LastName)
            .ToList();
    }

    public async Task<IActionResult> OnPost()
    {
        if (string.IsNullOrWhiteSpace(AssetNumber) && string.IsNullOrWhiteSpace(SerialNumber))
        {
            ModelState.AddModelError(nameof(AssetNumber), "You must enter either an Asset Number or Serial Number");
            ModelState.AddModelError(nameof(SerialNumber), "You must enter either an Asset Number or Serial Number");

            await PreparePage();

            return Page();
        }

        Result<SchoolResponse> school = await _mediator.Send(new GetSchoolByIdQuery(CurrentSchoolCode));

        if (UserType == StocktakeSighting.UserTypes.School)
        {
            UserName = school.Value.Name;
            UserCode = school.Value.SchoolCode;
        }

        RegisterSightingCommand command = new(
            EventId,
            SerialNumber,
            AssetNumber,
            Description,
            StocktakeSighting.LocationCategories.PublicSchool,
            school.Value.Name,
            school.Value.SchoolCode,
            UserType,
            UserName,
            UserCode,
            Comment,
            _currentUserService.EmailAddress,
            DateTime.Now);

        _logger.Information("Requested to submit stocktake sighting by user {user} with data {@command}", _currentUserService.UserName, command);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            await PreparePage();

            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }

        return RedirectToPage("/Stocktake/Index", new { area = "Schools", EventId });
    }
}