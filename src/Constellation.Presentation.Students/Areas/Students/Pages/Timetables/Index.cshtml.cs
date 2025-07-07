namespace Constellation.Presentation.Students.Areas.Students.Pages.Timetables;

using Application.Common.PresentationModels;
using Application.Domains.Timetables.Timetables.Queries.GetStudentTimetableData;
using Application.Domains.Timetables.Timetables.Queries.GetStudentTimetableExport;
using Application.Models.Auth;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStudent)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StudentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Timetables;

    public StudentTimetableDataDto StudentTimetableData { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve timetable data by user {user}", _currentUserService.UserName);

        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to retrieve timetable data by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(StudentErrors.InvalidId);

            return;
        }

        StudentId studentId = StudentId.FromValue(new(studentIdClaimValue));
        
        Result<StudentTimetableDataDto> timetableRequest = await _mediator.Send(new GetStudentTimetableDataQuery(studentId));

        if (timetableRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), timetableRequest.Error, true)
                .Warning("Failed to retrieve timetable data by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(timetableRequest.Error);

            return;
        }

        StudentTimetableData = timetableRequest.Value;
    }

    public async Task<IActionResult> OnGetDownload()
    {
        _logger.Information("Requested to download timetable file by user {user}", _currentUserService.UserName);

        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to download timetable file by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                StudentErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Timetables/Index", values: new { area = "Students" }));

            return Page();
        }
        
        StudentId studentId = StudentId.FromValue(new(studentIdClaimValue));
        
        Result<FileDto> request = await _mediator.Send(new GetStudentTimetableExportQuery(studentId));

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to download timetable file by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            return Page();
        }

        return File(request.Value.FileData, request.Value.FileType, request.Value.FileName);
    }
    
}