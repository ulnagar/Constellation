namespace Constellation.Infrastructure.Templates.Views.Emails.Enrolments;

using Constellation.Core.Enums;
using Constellation.Core.Models.EnrolmentContext.Offer.Identifiers;
using Constellation.Infrastructure.Templates.Views.Shared;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed class EnrolmentOfferReminderEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Enrolments/EnrolmentOfferReminderEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public string FormLink => $"{BaseUrl}/Enrolments/Offer/{Id}";
    public required OfferId Id { get; init; }
    public required string Year { get; init; }
    public required Name ParentName { get; init; }
    public required Name StudentName { get; init; }
    public required Grade Grade { get; init; }
    public required DateTimeOffset RespondBy { get; init; }
}