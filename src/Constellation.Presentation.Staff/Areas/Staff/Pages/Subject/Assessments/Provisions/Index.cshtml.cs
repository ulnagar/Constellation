namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Assessments.Provisions;

using Application.Common.PresentationModels;
using Application.Domains.Assessments.Provisions.Models;
using Application.Domains.Assessments.Provisions.Queries.GetAssessmentProvisions;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[HasPermission(AuthPermission.Subjects_AssessmentsProvisions_View_Value)]
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assessments_Provisions;
    [ViewData] public string PageTitle => "Assessment Provisions List";

    public List<AssessmentProvisionResponse> Provisions = [];

    public async Task OnGet()
    {
        Result<List<AssessmentProvisionResponse>> provisions = await _mediator.Send(new GetAssessmentProvisionsQuery());

        if (provisions.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), provisions.Error, true)
                .Warning("Failed to retrieve list of Assessment Provisions by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(provisions.Error);

            return;
        }

        Provisions = provisions.Value;
    }
}