namespace Constellation.Presentation.Students.Areas.Students.Pages.Tutorials;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Tutorials.Requests.Queries.GetTutorialRequestsForStudent;
using Constellation.Application.Domains.Tutorials.Tutorials.Queries.GetTutorialsForStudent;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Extensions;
using Serilog;

[HasPermission(AuthPermission.StudentPortal_View_Value)]
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
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForStudentPortal();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Tutorials;

    public List<TutorialRequestResponse> Requests { get; set; } = [];
    public List<TutorialResponse> Tutorials { get; set; } = [];

    public async Task OnGet()
    {
        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to retrieve tutorial request data by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                StudentErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Tutorials/Index", values: new { area = "Students" }));

            return;
        }

        StudentId studentId = StudentId.FromValue(new(studentIdClaimValue));

        Result<List<TutorialRequestResponse>> tutorialRequests = await _mediator.Send(new GetTutorialRequestsForStudentQuery(studentId));

        if (tutorialRequests.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), tutorialRequests.Error, true)
                .Warning("Failed to retrieve tutorial request data by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                tutorialRequests.Error,
                _linkGenerator.GetPathByPage("/Tutorials/Index", values: new { area = "Students" }));

            return;
        }

        Requests = tutorialRequests.Value
            .OrderByDescending(entry => entry.RequestDate)
            .ToList();

        Result<List<TutorialResponse>> tutorials = await _mediator.Send(new GetTutorialsForStudentQuery(studentId));

        if (tutorials.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), tutorials.Error, true)
                .Warning("Failed to retrieve tutorial data by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                tutorialRequests.Error,
                _linkGenerator.GetPathByPage("/Tutorials/Index", values: new { area = "Students" }));

            return;
        }

        Tutorials = tutorials.Value;
    }
}