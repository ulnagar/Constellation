namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Consent.Responses;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.ThirdPartyConsent.GetTransactionDetails;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Consent_Transactions;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public TransactionDetailsResponse Transaction { get; set; }

    public async Task OnGet()
    {
        ConsentTransactionId transactionId = ConsentTransactionId.FromValue(Id);

        Result<TransactionDetailsResponse> result = await _mediator.Send(new GetTransactionDetailsQuery(transactionId));

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Consent/Responses/Index", values: new { area = "Staff" }));

            return;
        }

        Transaction = result.Value;
    }
}