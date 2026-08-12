namespace Constellation.Infrastructure.Templates.Views.Emails.Enrolments;

using Constellation.Core.Enums;
using Constellation.Core.Models.EnrolmentContext.Offer.Identifiers;
using Constellation.Infrastructure.Templates.Views.Shared;
using Core.Models.EnrolmentContext.Offer.Enums;
using Core.ValueObjects;
using System;

public sealed class EnrolmentOfferResponseReceiptEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Enrolments/EnrolmentOfferResponseReceiptEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required OfferId Id { get; init; }
    public required string Year { get; init; }
    public required Name ParentName { get; init; }
    public required Name StudentName { get; init; }
    public required Grade Grade { get; init; }
    public required DateTimeOffset RespondedAt { get; init; }
    public required OfferStatus Status { get; init; }
    public required bool HasCourtOrders { get; init; }
    public required bool HasHealthConcerns { get; init; }
    public required bool LaptopRequested { get; init; }
}
