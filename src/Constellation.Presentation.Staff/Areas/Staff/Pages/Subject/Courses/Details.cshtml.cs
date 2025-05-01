namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Courses;

using Application.Common.PresentationModels;
using Constellation.Application.Domains.Courses.Queries.GetCourseDetails;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Courses_Courses;
    [ViewData] public string PageTitle { get; set; } = "Course Details";

    [BindProperty(SupportsGet = true)]
    public CourseId Id { get; set; } = CourseId.Empty;

    public CourseDetailsResponse Course { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve details of Course with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<CourseDetailsResponse> request = await _mediator.Send(new GetCourseDetailsQuery(Id));

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve details of Course with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Courses/Index", values: new { area = "Staff" }));

            return;
        }

        Course = request.Value;
        PageTitle = $"Course - {Course.Grade.AsNumber()}{Course.Code}";
    }
}
