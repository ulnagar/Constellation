namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Timetables;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Timetables.GetStudentTimetableExport;
using Constellation.Application.DTOs;
using Constellation.Application.Timetables.GetStudentTimetableData;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class ViewModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ViewModel(
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
            .ForContext<ViewModel>()
            .ForContext("APPLICATION", "Schools Portal");
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
            ModalContent = new ErrorDisplay(timetableRequest.Error);

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
            ModalContent = new ErrorDisplay(request.Error);

            return Page();
        }

        return File(request.Value.FileData, request.Value.FileType, request.Value.FileName);
    }
    
}