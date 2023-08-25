namespace Constellation.Presentation.Server.Areas.Subject.Pages.Offerings;

using Constellation.Application.Enrolments.UnenrolStudent;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingDetails;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Pages.Shared.PartialViews.DeleteRoleModal;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _sender;

    public DetailsModel(
        ISender sender)
    {
        _sender = sender;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public OfferingDetailsResponse Offering { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_sender);

        OfferingId offeringId = OfferingId.FromValue(Id);

        Result<OfferingDetailsResponse> query = await _sender.Send(new GetOfferingDetailsQuery(offeringId));

        if (query.IsFailure)
        {
            Error = new()
            {
                Error = query.Error,
                RedirectPath = null
            };

            return;
        }

        Offering = query.Value;
    }

    public IActionResult OnPostAjaxUnenrolConfirmation(
        string studentId,
        string studentName,
        string courseName,
        string offeringName)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        UnenrolStudentModalViewModel viewModel = new(
            offeringId,
            studentId,
            studentName,
            courseName,
            offeringName);

        return Partial("UnenrolStudentModal", viewModel);
    }

    public async Task<IActionResult> OnGetUnenrolStudent(
        string studentId)
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        Result request = await _sender.Send(new UnenrolStudentCommand(studentId, offeringId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage();
    }
}
