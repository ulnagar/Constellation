namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations;

using Application.Common.PresentationModels;
using Constellation.Application.Awards.CreateNominationPeriod;
using Constellation.Application.Awards.GetNominationPeriod;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanAddAwards)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Nominations;

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    public string Name { get; set; } = string.Empty;
    [BindProperty]
    public DateOnly LockoutDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    public List<Grade> Grades { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        if (Id.HasValue)
        {
            // This is an edit action
            AwardNominationPeriodId periodId = AwardNominationPeriodId.FromValue(Id.Value);

            Result<NominationPeriodDetailResponse> details = await _mediator.Send(new GetNominationPeriodRequest(periodId), cancellationToken);

            if (details.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    details.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = Id.Value }));

                return;
            }

            Name = details.Value.Name;
            LockoutDate = details.Value.LockoutDate;
            Grades = details.Value.IncludedGrades;
        }
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        if (!Id.HasValue && !Grades.Any())
            ModelState.AddModelError("Grades", "You must select at least one grade");

        if (!ModelState.IsValid) 
            return Page();

        if (Id.HasValue)
        {
            // This is an edit action
            AwardNominationPeriodId periodId = AwardNominationPeriodId.FromValue(Id.Value);

            Result editRequest = await _mediator.Send(new UpdateNominationPeriodCommand(periodId, Name, LockoutDate), cancellationToken);

            if (editRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(editRequest.Error);

                return Page();
            }

            return RedirectToPage("/SchoolAdmin/Awards/Nominations/Details", new { area = "Staff", PeriodId = Id.Value });
        }

        Result request = await _mediator.Send(new CreateNominationPeriodCommand(Name, LockoutDate, Grades), cancellationToken);

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Awards/Nominations/Index", new { area = "Staff" });
    }
}
