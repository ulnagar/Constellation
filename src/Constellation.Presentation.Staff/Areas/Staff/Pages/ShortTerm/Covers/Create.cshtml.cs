namespace Constellation.Presentation.Staff.Areas.Staff.Pages.ShortTerm.Covers;

using Application.Domains.Casuals.Queries.GetCasualsForSelectionList;
using Application.Domains.Covers.Commands.BulkCreateCovers;
using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Application.Domains.StaffMembers.Queries.GetStaffLinkedToOffering;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Offerings.Queries.GetOfferingsForSelectionList;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Presentation.Staff.Areas.Staff.Models;
using Core.Abstractions.Services;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanEditCovers)]
public class CreateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public CreateModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<CreateModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.ShortTerm_Covers_Index;
    [ViewData] public string PageTitle => "New Class Cover";


    [BindProperty]
    public string CoveringTeacherId { get; set; }
    [BindProperty]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]

    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    public List<Guid> CoveredClasses { get; set; } = new();
    [BindProperty]
    [ModelBinder(typeof(BaseFromNameBinder))]
    public CoverType CoverType { get; set; } = CoverType.ClassCover;

    [BindProperty]
    public string Note { get; set; } = "Sample Note";

    public List<CoveringTeacherRecord> CoveringTeacherSelectionList { get; set; } = new();
    public List<ClassRecord> ClassSelectionList { get; set; } = new();
    public IEnumerable<CoverType> CoverTypeList = CoverType.GetOptions;

    public async Task OnGet(CancellationToken cancellationToken) => await PreparePage(cancellationToken);

    public async Task<IActionResult> OnPostCreate(CancellationToken cancellationToken)
    {
        await PreparePage(cancellationToken);
        
        CoveringTeacherRecord teacher = CoveringTeacherSelectionList.First(entry => entry.Id == CoveringTeacherId);

        CoverTeacherType? teacherType = teacher.Category switch
        {
            "Casuals" => CoverTeacherType.Casual,
            "Teachers" => CoverTeacherType.Staff,
            _ => null
        };

        List<OfferingId> offeringIds = CoveredClasses.Select(OfferingId.FromValue).ToList();

        BulkCreateCoversCommand command = new(
            offeringIds,
            StartDate,
            EndDate,
            teacherType,
            teacher.Id,
            CoverType,
            Note);

        _logger
            .ForContext(nameof(BulkCreateCoversCommand), command, true)
            .Information("Requested to create Class Covers by user {User}", _currentUserService.UserName);

        Result<List<Cover>> result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to create Class Covers by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);
        
            return Page();
        }

        return RedirectToPage("/ShortTerm/Covers/Index", new { area = "Staff" });
    }

    private async Task PreparePage(CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to retrieve defaults for new Class Cover by user {User}", _currentUserService.UserName);

        Result<List<StaffSelectionListResponse>> teacherResponse = await _mediator.Send(new GetStaffForSelectionListQuery(), cancellationToken);

        if (teacherResponse.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), teacherResponse.Error, true)
                .Warning("Failed to retrieve defaults for new Class Cover by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                teacherResponse.Error,
                _linkGenerator.GetPathByPage("/ShortTerm/Covers/Index", values: new { area = "Staff" }));

            return;
        }

        Result<List<CasualsSelectionListResponse>> casualResponse = await _mediator.Send(new GetCasualsForSelectionListQuery(), cancellationToken);

        if (casualResponse.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), casualResponse.Error, true)
                .Warning("Failed to retrieve defaults for new Class Cover by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                casualResponse.Error,
                _linkGenerator.GetPathByPage("/ShortTerm/Covers/Index", values: new { area = "Staff" }));
            
            return;
        }

        CoveringTeacherSelectionList.AddRange(teacherResponse
            .Value
            .Select(teacher =>
                new CoveringTeacherRecord(
                    teacher.StaffId.ToString(),
                    teacher.Name.DisplayName,
                    teacher.Name.SortOrder,
                    "Teachers"
                ))
            .ToList());

        CoveringTeacherSelectionList.AddRange(casualResponse
            .Value
            .Select(casual =>
                new CoveringTeacherRecord(
                    casual.Id.ToString(),
                    casual.Name.DisplayName,
                    casual.Name.SortOrder,
                    "Casuals"
                ))
            .ToList());

        CoveringTeacherSelectionList = CoveringTeacherSelectionList
            .OrderBy(entry => entry.Category)
            .ThenBy(entry => entry.SortName)
            .ToList();

        Result<List<OfferingSelectionListResponse>> classesResponse = await _mediator.Send(new GetOfferingsForSelectionListQuery(), cancellationToken);

        if (classesResponse.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), classesResponse.Error, true)
                .Warning("Failed to retrieve defaults for new Class Cover by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                classesResponse.Error,
                _linkGenerator.GetPathByPage("/ShortTerm/Covers/Index", values: new { area = "Staff" }));

            return;
        }

        foreach (OfferingSelectionListResponse course in classesResponse.Value)
        {
            Result<List<StaffSelectionListResponse>> teachers = await _mediator.Send(new GetStaffLinkedToOfferingQuery(course.Id), cancellationToken);

            if (teachers.Value.Count == 0)
                continue;

            ClassSelectionList.Add(new ClassRecord(
                course.Id,
                course.Name,
                $"{teachers.Value.First().Name.FirstName[..1]} {teachers.Value.First().Name.LastName}",
                $"Year {course.Name[..2]}"));
        }
    }
    
    public sealed record CoveringTeacherRecord(
        string Id,
        string DisplayName,
        string SortName,
        string Category);
}
