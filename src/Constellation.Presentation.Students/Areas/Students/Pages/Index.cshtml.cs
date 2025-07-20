namespace Constellation.Presentation.Students.Areas.Students.Pages;

using Application.Domains.Offerings.Queries.GetCurrentOfferingsAndTutorialsForStudent;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStudent)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StudentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Dashboard;

    public List<DetailResponse> Offerings { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve Student Dashboard by user {User}", _currentUserService.UserName);

        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to retrieve Student Dashboard by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(StudentErrors.InvalidId);

            return;
        }

        StudentId studentId = StudentId.FromValue(new(studentIdClaimValue));
        
        Result<List<DetailResponse>> request = await _mediator.Send(new GetCurrentOfferingsAndTutorialsForStudentQuery(studentId));

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve Student Dashboard by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            return;
        }

        Offerings = request.Value;
    }
}
