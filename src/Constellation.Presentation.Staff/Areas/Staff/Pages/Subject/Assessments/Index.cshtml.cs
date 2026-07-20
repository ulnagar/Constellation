namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Assessments;

using Application.Common.PresentationModels;
using Application.Domains.Assessments.Assessments.Models;
using Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessments;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Serilog;

[HasPermission(AuthPermission.Subjects_Assessments_View_Value)]
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
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assessments_Assessments;
    [ViewData] public string PageTitle => "Assessments";

    public List<AssessmentResponse> Assessments = [];

    public async Task OnGet()
    {
        Result<List<AssessmentResponse>> assessments = await _mediator.Send(new GetCurrentAssessmentsQuery());

        if (assessments.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), assessments.Error, true)
                .Warning("Failed to retrieve current Assessments by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(assessments.Error);

            return;
        }

        Assessments = assessments.Value;
    }
}