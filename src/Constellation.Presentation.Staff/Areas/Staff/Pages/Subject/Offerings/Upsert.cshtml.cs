namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Offerings;

using Application.Common.PresentationModels;
using Constellation.Application.Courses.GetCoursesForSelectionList;
using Constellation.Application.Courses.Models;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.CreateOffering;
using Constellation.Application.Offerings.GetOfferingSummary;
using Constellation.Application.Offerings.Models;
using Constellation.Application.Offerings.UpdateOffering;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IDateTimeProvider _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IDateTimeProvider dateTime,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Offerings_Offerings;
    [ViewData] public string PageTitle { get; set; } = "New Offering";

    [BindProperty(SupportsGet = true)]
    public OfferingId Id { get; set; } = OfferingId.Empty;

    [BindProperty]
    public string Name { get; set; }
    [BindProperty]
    public CourseId CourseId { get; set; } = CourseId.Empty;
    public string CourseName { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly StartDate { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly EndDate { get; set; }
    public SelectList CourseList { get; set; }

    public async Task OnGet()
    {
        if (Id != OfferingId.Empty)
        {
            _logger.Information("Requested to retrieve details of Offering with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            Result<OfferingSummaryResponse> offering = await _mediator.Send(new GetOfferingSummaryQuery(Id));

            if (offering.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), offering.Error, true)
                    .Warning("Failed to retrieve details of Offering with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    offering.Error,
                    _linkGenerator.GetPathByPage("/Subject/Offerings/Index", values: new { area = "Staff" }));

                return;
            }

            Name = offering.Value.Name;
            CourseName = offering.Value.CourseName;
            StartDate = offering.Value.StartDate;
            EndDate = offering.Value.EndDate;

            PageTitle = $"Edit - {Name}";
        }
        else
        {
            StartDate = _dateTime.Today;
            EndDate = _dateTime.LastDayOfYear;

            await BuildCourseSelectList();
        }
    }

    public async Task<IActionResult> OnPostCreate()
    {
        if (!ModelState.IsValid)
        {
            await BuildCourseSelectList();

            return Page();
        }

        CreateOfferingCommand command = new(Name, CourseId, StartDate, EndDate);

        _logger
            .ForContext(nameof(CreateOfferingCommand), command, true)
            .Information("Requested to create new Offering by user {User}", _currentUserService.UserName);

        Result<OfferingId> request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to create new Offering by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Index", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage("/Subject/Offerings/Details", new { area = "Staff", Id = request.Value.Value });
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (!ModelState.IsValid)
            return Page();

        UpdateOfferingCommand command = new(Id, StartDate, EndDate);

        _logger
            .ForContext(nameof(UpdateOfferingCommand), command, true)
            .Information("Requested to update Offering by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to update Offering by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id.Value }));

            return Page();
        }

        return RedirectToPage("/Subject/Offerings/Details", new { area = "Staff", Id = Id.Value });
    }

    private async Task BuildCourseSelectList()
    {
        Result<List<CourseSelectListItemResponse>> coursesResponse = await _mediator.Send(new GetCoursesForSelectionListQuery());

        if (coursesResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(coursesResponse.Error);

            return;
        }

        CourseList = new SelectList(coursesResponse.Value, "Id", "DisplayName", null, "FacultyName");
    }
}
