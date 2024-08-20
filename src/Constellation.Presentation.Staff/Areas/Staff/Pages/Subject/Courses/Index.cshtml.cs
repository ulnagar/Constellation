namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Courses;

using Application.Common.PresentationModels;
using Constellation.Application.Courses.GetCourseSummaryList;
using Constellation.Application.Courses.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Courses_Courses;
    [ViewData] public string PageTitle => "Course List";

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Active;

    public List<CourseSummaryResponse> Courses { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of Courses by user {User}", _currentUserService.UserName);

        Result<List<CourseSummaryResponse>> request = await _mediator.Send(new GetCourseSummaryListQuery());

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve list of Courses by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(request.Error);

            return;
        }

        Courses = Filter switch
        {
            FilterDto.All => request.Value,
            FilterDto.Active => request.Value.Where(course => course.Offerings.Any(offering => offering.IsCurrent)).ToList(),
            FilterDto.Inactive => request.Value.Where(course => course.Offerings.All(offering => !offering.IsCurrent)).ToList(),
            _ => request.Value
        };
    }

    public enum FilterDto
    {
        All,
        Active,
        Inactive
    }
}
