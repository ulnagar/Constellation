namespace Constellation.Infrastructure.Templates.Views.Emails.Enrolments;

using Core.Enums;
using Core.Models.EnrolmentContext.Offer.Identifiers;
using Core.ValueObjects;
using Shared;
using System;

public sealed class EnrolmentOfferNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Enrolments/EnrolmentOfferNotificationEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public string FormLink => $"{BaseUrl}/Enrolments/Offer/{Id}";
    public required OfferId Id { get; init; }
    public required string Year { get; init; }
    public required Name ParentName { get; init; }
    public required Name StudentName { get; init; }
    public required Grade Grade { get; init; }
    public required DateTimeOffset RespondBy { get; init; }
    public List<string> Courses { get; set; } = [];

}
