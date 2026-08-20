namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Offers;

using Application.Domains.EnrolmentContext.Offers.Commands.AddOfferNote;
using Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferApproved;
using Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferDocumentsCollected;
using Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferRejected;
using Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferReviewCompleted;
using Application.Domains.EnrolmentContext.Offers.Queries.GetEnrolmentOfferById;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Core.Models.EnrolmentContext.Offer.Errors;
using Core.Models.EnrolmentContext.Offer.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Serilog;
using Shared.Components.AddOfferNote;

[HasPermission(AuthPermission.Partners_Enrolments_Offers_View_Value)]
public class DetailsModel : PeriodScopedPageModel
{
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
        : base(mediator)
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Offers;
    [ViewData] public string PageTitle => "Enrolment Offers";

    [BindProperty(SupportsGet = true)]
    public OfferId Id { get; set; } = OfferId.Empty;

    public EnrolmentOfferDetailsResponse Offer { get; set; }

    public async Task OnGet()
    {
        if (Id == OfferId.Empty)
        {
            ModalContent = ErrorDisplay.Create(
                EnrolmentOfferErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Offers/Index", values: new { area = "Staff", PeriodId }));

            return;
        }

        await PreparePage();
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to load Enrolment Offer details by user {User}", _currentUserService.UserName);

        Result<EnrolmentOfferDetailsResponse> offer = await _mediator.Send(new GetEnrolmentOfferByIdQuery(Id));

        if (offer.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), offer.Error, true)
                .Information("Failed to load Enrolment Offer details by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                offer.Error,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Offers/Index", values: new { area = "Staff", PeriodId }));

            return;
        }

        if (offer.Value.PeriodId != PeriodId)
        {
            _logger
                .ForContext(nameof(Error), EnrolmentPeriodErrors.PeriodMismatch, true)
                .Information("Failed to load Enrolment Offer details by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                EnrolmentPeriodErrors.PeriodMismatch,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Offers/Index", values: new { area = "Staff", PeriodId }));

            return;
        }

        Offer = offer.Value;
    }

    public async Task<IActionResult> OnPostAddNote(AddOfferNoteSelection viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.Note))
        {
            ModalContent = ErrorDisplay.Create(OfferNoteErrors.NoteEmpty);

            await PreparePage();
            return Page();
        }

        AddOfferNoteCommand command = new(Id, viewModel.Note, _currentUserService.UserName);

        _logger
            .ForContext(nameof(AddOfferNoteCommand), command, true)
            .Information("Requested to add new Offer Note by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(AddOfferNoteCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to add new Offer Note by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkDocumentsCollected()
    {
        MarkOfferDocumentsCollectedCommand command = new(Id, _currentUserService.UserName);

        _logger
            .ForContext(nameof(MarkOfferDocumentsCollectedCommand), command, true)
            .Information("Requested to mark Offer as Documents Collected by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(MarkOfferDocumentsCollectedCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to mark Offer as Documents Collected by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);
            await PreparePage();
            return Page();
        }
        
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkReviewCompleted()
    {
        MarkOfferReviewCompletedCommand command = new(Id, _currentUserService.UserName);

        _logger
            .ForContext(nameof(MarkOfferReviewCompletedCommand), command, true)
            .Information("Requested to mark Offer as Review Completed by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(MarkOfferReviewCompletedCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to mark Offer as Review Completed by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);
            await PreparePage();
            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkApproved()
    {
        MarkOfferApprovedCommand command = new(Id, _currentUserService.UserName);

        _logger
            .ForContext(nameof(MarkOfferApprovedCommand), command, true)
            .Information("Requested to mark Offer as Approved by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(MarkOfferApprovedCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to mark Offer as Approved by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);
            await PreparePage();
            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkRejected()
    {
        MarkOfferRejectedCommand command = new(Id, _currentUserService.UserName);

        _logger
            .ForContext(nameof(MarkOfferRejectedCommand), command, true)
            .Information("Requested to mark Offer as Rejected by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(MarkOfferRejectedCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to mark Offer as Rejected by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);
            await PreparePage();
            return Page();
        }

        return RedirectToPage();
    }
}