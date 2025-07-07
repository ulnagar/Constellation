namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Constellation.Application.Domains.Students.Queries.GetFilteredStudents;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public sealed class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Students;
    [ViewData] public string PageTitle => "Student List";

    [BindProperty(SupportsGet = true)] 
    public StudentFilter Filter { get; set; } = StudentFilter.Active;

    public List<FilteredStudentResponse> Students { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of Students by user {User}", _currentUserService.UserName);

        Result<List<FilteredStudentResponse>>? students = await _mediator.Send(new GetFilteredStudentsQuery(Filter));

        if (students.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), students.Error, true)
                .Warning("Failed to retrieve list of Students by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(students.Error);

            return;
        }

        Students = students.Value
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.StudentName.SortOrder)
            .ToList();
    }
}
