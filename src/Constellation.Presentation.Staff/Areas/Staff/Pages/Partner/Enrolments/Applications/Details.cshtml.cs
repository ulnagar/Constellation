namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Applications;

using Application.Domains.EnrolmentContext.Applications.Models;
using Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationById;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.Partners_Enrolments_Applications_View_Value)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Applications;
    [ViewData] public string PageTitle => "Enrolment Applications";

    [BindProperty(SupportsGet = true)]
    public ApplicationId Id { get; set; } = ApplicationId.Empty;

    public EnrolmentApplicationResponse Application { get; set; }

    public async Task OnGet()
    {
        if (Id == ApplicationId.Empty)
        {
            ModalContent = ErrorDisplay.Create(
                EnrolmentApplicationErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Applications/Index", values: new { area = "Staff" }));

            return;
        }

        await PreparePage();
    }

    private async Task PreparePage()
    {
        Result<EnrolmentApplicationResponse> application = await _mediator.Send(new GetEnrolmentApplicationByIdQuery(Id));

        if (application.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                application.Error,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Applications/Index", values: new { area = "Staff" }));

            return;
        }

        Application = application.Value;
    }
}