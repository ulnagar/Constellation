namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Timetables;

using Application.Common.PresentationModels;
using Application.Domains.Students.Queries.GetCurrentStudentsFromSchool;
using Application.Models.Auth;
using Constellation.Application.Domains.Students.Models;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Serilog;

[HasPermission(AuthPermission.SchoolsPortal_Timetables_View_Value)]
public class IndexModel : BasePageModel
{
    private ISender _mediator => Mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger) 
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForSchoolPortal();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Timetables;

    public List<StudentResponse> Students { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve student list by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);

        Result<List<StudentResponse>> students = await _mediator.Send(new GetCurrentStudentsFromSchoolQuery(CurrentSchoolCode));

        if (students.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(students.Error);

            return;
        }

        Students = students.Value
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.Name.SortOrder)
            .ToList();
    }
}