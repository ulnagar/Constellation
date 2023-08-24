namespace Constellation.Presentation.Server.Areas.Subject.Pages.Offerings;

using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingDetails;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
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
}
