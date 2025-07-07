namespace Constellation.Presentation.Schools.Areas.Schools.Pages.ExamSubmission;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Constellation.Application.Domains.Students.Queries.GetStudentsFromSchoolForSelectionList;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class Step1Model : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public Step1Model(
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
            .ForContext<Step1Model>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Exams;

    public SelectList Students { get; set; }
    
    public string StudentId { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve student list by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);

        Result<List<StudentSelectionResponse>> studentsRequest = await _mediator.Send(new GetStudentsFromSchoolForSelectionQuery(CurrentSchoolCode));

        if (studentsRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(studentsRequest.Error);

            return;
        }

        List<StudentSelectionResponse> students = studentsRequest.Value
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        Students = new SelectList(students, 
            nameof(StudentSelectionResponse.StudentId),
            nameof(StudentSelectionResponse.DisplayName),
            null,
            nameof(StudentSelectionResponse.CurrentGrade));
    }
}