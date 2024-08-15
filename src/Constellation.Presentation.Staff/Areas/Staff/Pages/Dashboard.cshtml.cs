#nullable enable
namespace Constellation.Presentation.Staff.Areas.Staff.Pages;

using Application.Affirmations;
using Application.Stocktake.GetCurrentStocktakeEvents;
using Application.Training.GetCountOfExpiringCertificatesForStaffMember;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;
using Constellation.Application.StaffMembers.GetStaffByEmail;
using Constellation.Application.StaffMembers.Models;
using Constellation.Application.Stocktake.Models;
using Constellation.Core.Shared;
using Core.Models.Offerings.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DashboardModel : BasePageModel
{
    private readonly ISender _mediator;

    public DashboardModel(ISender mediator)
    {
        _mediator = mediator;
    }

    public string UserName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public string StaffId { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public IReadOnlyList<StocktakeEventResponse> ActiveStocktakeEvents { get; set; } =
        new List<StocktakeEventResponse>();

    public int ExpiringTraining { get; set; } = 0;

    public Dictionary<string, OfferingId> Classes { get; set; } = new();

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken = default)
    {
        string? username = User.Identity?.Name;

        if (username is not null)
        {
            Result<List<TeacherOfferingResponse>> query = await _mediator.Send(new GetCurrentOfferingsForTeacherQuery(null, username));

            if (query.IsSuccess)
                Classes = query.Value.ToDictionary(k => k.OfferingName.Value, k => k.OfferingId);
        }

        IsAdmin = User.IsInRole(AuthRoles.Admin);

        Result<StaffSelectionListResponse> teacherRequest = await _mediator.Send(new GetStaffByEmailQuery(username), cancellationToken);

        Result<string> messageRequest = await _mediator.Send(new GetAffirmationQuery(teacherRequest.Value?.StaffId), cancellationToken);

        if (messageRequest.IsSuccess)
            Message = messageRequest.Value;

        Result<List<StocktakeEventResponse>>? stocktakeEvents = await _mediator.Send(new GetCurrentStocktakeEventsQuery(), cancellationToken);
        ActiveStocktakeEvents = stocktakeEvents.IsSuccess ? stocktakeEvents.Value : new List<StocktakeEventResponse>();

        if (teacherRequest.IsFailure)
            return Page();

        StaffId = teacherRequest.Value!.StaffId;
        UserName = $"{teacherRequest.Value.FirstName} {teacherRequest.Value.LastName}";

        Result<int> trainingExpiringSoonRequest = await _mediator.Send(new GetCountOfExpiringCertificatesForStaffMemberQuery(StaffId), cancellationToken);

        if (trainingExpiringSoonRequest.IsSuccess)
            ExpiringTraining = trainingExpiringSoonRequest.Value;

        return Page();
    }
}
