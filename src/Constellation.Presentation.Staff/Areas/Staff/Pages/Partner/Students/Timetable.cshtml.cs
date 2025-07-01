namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students;

using Application.Domains.Timetables.Timetables.Queries.GetStudentTimetableData;
using Application.Domains.Timetables.Timetables.Queries.GetStudentTimetableExport;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]

public sealed class TimetableModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public TimetableModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<TimetableModel>();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Students;
    [ViewData] public string PageTitle => "Student Timetable";

    [BindProperty(SupportsGet = true)]
    public StudentId Id { get; set; } = StudentId.Empty;

    public StudentTimetableDataDto StudentTimetableData { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve timetable data by user {user} for student {student}", _currentUserService.UserName, Id);

        Result<StudentTimetableDataDto> timetableRequest = await _mediator.Send(new GetStudentTimetableDataQuery(Id));

        if (timetableRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(timetableRequest.Error);

            return;
        }

        StudentTimetableData = timetableRequest.Value;
    }

    public async Task<IActionResult> OnGetDownload()
    {
        _logger.Information("Requested to download timetable file by user {user} for student {student}", _currentUserService.UserName, Id);

        Result<FileDto> request = await _mediator.Send(new GetStudentTimetableExportQuery(Id));

        if (request.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(request.Error);

            return Page();
        }

        return File(request.Value.FileData, request.Value.FileType, request.Value.FileName);
    }
}