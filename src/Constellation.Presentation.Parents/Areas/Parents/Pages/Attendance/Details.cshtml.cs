namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Attendance;

using Application.Absences.ProvideParentWholeAbsenceExplanation;
using Application.Models.Auth;
using Constellation.Application.Absences.GetAbsenceDetailsForParent;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Models.Absences;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.IsParent)]
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
            .ForContext("APPLICATION", "Parent Portal");
    }

    [ViewData] public string ActivePage => Models.ActivePage.Attendance;

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public AbsenceId Id { get; set; }

    public ParentAbsenceDetailsResponse? Absence { get; set; }

    [BindProperty]
    public string Comment { get; set; } = string.Empty;
    [BindProperty]
    public bool CanBeExplainedByParent { get; set; }


    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost()
    {
        if (string.IsNullOrWhiteSpace(Comment) || Comment.Length < 5)
        {
            ModelState.TryAddModelError(nameof(Comment), "You must include a longer comment");

            await PreparePage();

            return Page();
        }

        ProvideParentWholeAbsenceExplanationCommand command = new(Id, Comment)
        {
            ParentEmail = _currentUserService.EmailAddress
        };

        Result commandRequest = await _mediator.Send(command);

        if (commandRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(commandRequest.Error);

            await PreparePage();

            return Page();
        }

        if (!CanBeExplainedByParent)
        {
            ModalContent = new FeedbackDisplay(
                "Absence Explanation Forwarded",
                "Your explanation for this absence has been forwarded to the school office for entry.",
                "Ok",
                "btn-success",
                _linkGenerator.GetPathByPage("/Attendance/Absences", values: new { area = "Parents" }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Attendance/Absences", new { area = "Parents" });
    }

    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve absence details by user {user} with Id {id}", _currentUserService.UserName, Id);

        Result<ParentAbsenceDetailsResponse> absenceRequest = await _mediator.Send(new GetAbsenceDetailsForParentQuery(_currentUserService.EmailAddress, Id));

        if (absenceRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(absenceRequest.Error);

            return;
        }

        Absence = absenceRequest.Value;
        CanBeExplainedByParent = Absence.CanBeExplainedByParent;
    }
}