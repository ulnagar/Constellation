namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Absences;

using Constellation.Application.Absences.GetAbsenceDetails;
using Constellation.Application.Absences.SendAbsenceNotificationToParent;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    public Guid Id { get; set; }

    public AbsenceDetailsResponse Absence;

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        await GetClasses(_mediator);

        AbsenceId absenceId = AbsenceId.FromValue(Id);

        var result = await _mediator.Send(new GetAbsenceDetailsQuery(absenceId), cancellationToken);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = Request.Headers["Referer"].ToString()
            };

            return;
        }

        Absence = result.Value;
    }

    public async Task<IActionResult> OnGetSendNotification(string studentId, CancellationToken cancellationToken = default)
    {
        AbsenceId absenceId = AbsenceId.FromValue(Id);

        await _mediator.Send(new SendAbsenceNotificationToParentCommand(Guid.NewGuid(), studentId, new List<AbsenceId> { absenceId }), cancellationToken);

        return RedirectToPage();
    }
}
