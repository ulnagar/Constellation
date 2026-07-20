namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Timetables;

using Application.Common.PresentationModels;
using Application.Domains.Timetables.Timetables.Queries.GetStudentTimetableData;
using Application.Domains.Timetables.Timetables.Queries.GetStudentTimetableExport;
using Application.Models.Auth;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Serilog;

[HasPermission(AuthPermission.SchoolsPortal_Timetables_View_Value)]
public class ViewModel : BasePageModel
{
    private ISender _mediator => Mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ViewModel(
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger) 
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ViewModel>()
            .ForSchoolPortal();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Timetables;

    [BindProperty(SupportsGet = true)]
    public StudentId StudentId { get; set; }

    public StudentTimetableDataDto StudentTimetableData { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve timetable data by user {user} for student {student}", _currentUserService.UserName, StudentId);

        Result<StudentTimetableDataDto> timetableRequest = await _mediator.Send(new GetStudentTimetableDataQuery(StudentId));

        if (timetableRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(timetableRequest.Error);

            return;
        }

        StudentTimetableData = timetableRequest.Value;
    }

    public async Task<IActionResult> OnGetDownload()
    {
        _logger.Information("Requested to download timetable file by user {user} for student {student}", _currentUserService.UserName, StudentId);

        Result<FileDto> request = await _mediator.Send(new GetStudentTimetableExportQuery(StudentId));

        if (request.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(request.Error);

            return Page();
        }

        return File(request.Value.FileData, request.Value.FileType, request.Value.FileName);
    }
    
}