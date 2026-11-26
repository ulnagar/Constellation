namespace Constellation.Infrastructure.Templates.Views.Emails.Enrolments;

using Constellation.Core.ValueObjects;
using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class EnrolmentUserRegistrationConfirmationEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Enrolments/EnrolmentUserRegistrationConfirmationEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required string Link { get; init; }
    public required Name UserName { get; init; }

}