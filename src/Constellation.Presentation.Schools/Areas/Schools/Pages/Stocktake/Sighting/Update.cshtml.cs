namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Stocktake.Sighting;

using Application.Domains.AssetManagement.Stocktake.Commands.RegisterSightingWithAssetRecordUpdates;
using Application.Domains.Students.Models;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.AssetManagement.Stocktake.Commands.RegisterManualSighting;
using Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetAssetForSightingConfirmation;
using Constellation.Application.Domains.Schools.Queries.GetSchoolById;
using Constellation.Application.Domains.StaffMembers.Models;
using Constellation.Application.Domains.StaffMembers.Queries.GetStaffFromSchool;
using Constellation.Application.Domains.Students.Queries.GetCurrentStudentsFromSchool;
using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Core.Abstractions.Services;
using Core.Models.Stocktake.Enums;
using Core.Models.Stocktake.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class UpdateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public UpdateModel(
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
            .ForContext<UpdateModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Stocktake;

    [BindProperty(SupportsGet = true)] 
    public StocktakeEventId EventId { get; set; } = StocktakeEventId.Empty;

    [BindProperty(SupportsGet = true)]
    public AssetNumber AssetNumber { get; set; } = AssetNumber.Empty;

    public AssetSightingResponse Asset { get; set; }

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

        RegisterSightingWithAssetRecordUpdatesCommand command = new(
            EventId,
            AssetNumber,
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

            ModalContent = ErrorDisplay.Create(result.Error);

            return Page();
        }

        return RedirectToPage("/Stocktake/Index", new { area = "Schools", EventId });
    }

    private async Task PreparePage()
    {
        if (AssetNumber == AssetNumber.Empty)
        {
            ModalContent = ErrorDisplay.Create(
                AssetNumberErrors.Empty,
                _linkGenerator.GetPathByPage("/Stocktake/Index", values: new { area = "Schools", Id = EventId }));
            
            return;
        }

        Result<AssetSightingResponse> asset = await _mediator.Send(new GetAssetForSightingConfirmationQuery(AssetNumber, string.Empty));

        if (asset.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                asset.Error,
                _linkGenerator.GetPathByPage("/Stocktake/Index", values: new { area = "Schools", Id = EventId }));
                
            return;
        }

        Asset = asset.Value;

        Result<List<StudentResponse>> students = await _mediator.Send(new GetCurrentStudentsFromSchoolQuery(CurrentSchoolCode));

        if (students.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                students.Error,
                _linkGenerator.GetPathByPage("/Stocktake/Index", values: new { area = "Schools", Id = EventId }));

            return;
        }

        StudentList = students.Value
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.Name.SortOrder)
            .ToList();

        Result<List<StaffSelectionListResponse>> teachers = await _mediator.Send(new GetStaffFromSchoolQuery(CurrentSchoolCode));

        if (teachers.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                teachers.Error,
                _linkGenerator.GetPathByPage("/Stocktake/Index", values: new { area = "Schools", Id = EventId }));

            return;
        }

        StaffList = teachers.Value
            .OrderBy(teacher => teacher.Name.SortOrder)
            .ToList();
    }
}
