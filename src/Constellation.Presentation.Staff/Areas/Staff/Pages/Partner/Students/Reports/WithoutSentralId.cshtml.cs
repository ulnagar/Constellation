namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Reports;

using Application.Common.PresentationModels;
using Application.GroupTutorials.GenerateTutorialAttendanceReport;
using Application.Models.Auth;
using Application.Students.GetCurrentStudentsWithoutSentralId;
using Application.Students.Models;
using Application.Students.UpdateStudentSentralId;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class WithoutSentralIdModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public WithoutSentralIdModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<WithoutSentralIdModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Reports;
    [ViewData] public string PageTitle => "Students without Sentral Ids";

    public List<StudentResponse> Students { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of Students without Sentral Ids by user {User}", _currentUserService.UserName);

        Result<List<StudentResponse>> request = await _mediator.Send(new GetCurrentStudentsWithoutSentralIdQuery());

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve list of Students without Sentral Ids by user {User}", _currentUserService.UserName);

            return;
        }

        Students = request.Value;
    }

    public async Task<IActionResult> OnGetUpdateSentralIds()
    {
        _logger.Information("Requested to update Sentral Id data for Students by user {User}", _currentUserService.UserName);

        Result<List<StudentResponse>> request = await _mediator.Send(new GetCurrentStudentsWithoutSentralIdQuery());

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            return Page();
        }

        Students = request.Value;

        foreach (StudentResponse student in Students)
        {
            await _mediator.Send(new UpdateStudentSentralIdCommand(student.StudentId));
        }

        return RedirectToPage();
    }
}