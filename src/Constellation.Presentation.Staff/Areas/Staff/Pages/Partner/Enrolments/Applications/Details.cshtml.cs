namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Applications;

using Application.Domains.EnrolmentContext.Applications.Commands.UpdateEnrolmentApplicationStatus;
using Application.Domains.EnrolmentContext.Applications.Models;
using Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationById;
using Application.Domains.EnrolmentContext.Offers.Commands.CreateOfferFromApplication;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Abstractions.Services;
using Core.Models.EnrolmentContext.Application.Enums;
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
        _logger.Information("Requested to load Enrolment Application details by user {User}", _currentUserService.UserName);

        Result<EnrolmentApplicationResponse> application = await _mediator.Send(new GetEnrolmentApplicationByIdQuery(Id));

        if (application.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), application.Error, true)
                .Information("Failed to load Enrolment Application details by user {User}", _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(
                application.Error,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Applications/Index", values: new { area = "Staff" }));

            return;
        }

        Application = application.Value;
    }

    public async Task<IActionResult> OnPostUpdateStatus(ApplicationStatus status)
    {
        UpdateEnrolmentApplicationStatusCommand command = new(Id, status);

        _logger
            .ForContext(nameof(UpdateEnrolmentApplicationStatusCommand), command, true)
            .Information("Requested to update Enrolment Application status by user {User}",
                _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateEnrolmentApplicationStatusCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Information("Failed to update Enrolment Application status by user {User}",
                    _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }
        
        return RedirectToPage("/Partner/Enrolments/Applications/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnPostCreateOffer()
    {
        _logger
            .Information("Requested to create Offer from Enrolment Application by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(new CreateOfferFromApplicationCommand(Id));

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Information("Failed to create Offer from Enrolment Application by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage();
    }
}