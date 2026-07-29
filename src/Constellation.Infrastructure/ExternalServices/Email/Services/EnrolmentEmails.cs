namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Constellation.Core.Shared;
using Constellation.Infrastructure.Templates.Views.Emails.Enrolments;
using Core.Errors;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Offer;
using Core.ValueObjects;

public sealed partial class Service : IEmailService
{
    public async Task<Result> SendEnrolmentOfferNotification(
        Application application,
        Offer offer,
        string year,
        CancellationToken cancellationToken = default)
    {
        EnrolmentOfferNotificationEmailViewModel viewModel = new()
        {
            Title = $"Assessment Submission Received",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            Id = offer.Id,
            Grade = application.Grade,
            ParentName = application.ParentName,
            StudentName = application.StudentName,
            RespondBy = offer.RespondBy.Value,
            Year = year
        };

        if (application.ParentName is null)
            return Result.Failure(ApplicationErrors.ArgumentNull(nameof(Application.ParentName)));

        if (application.ParentEmailAddress is null)
            return Result.Failure(ApplicationErrors.ArgumentNull(nameof(Application.ParentEmailAddress)));

        Result<EmailRecipient> recipient = EmailRecipient.Create(application.ParentName, application.ParentEmailAddress);

        if (recipient.IsFailure)
            return recipient;

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Enrolments",
            viewModel.Title,
            [recipient.Value],
            cancellationToken: cancellationToken);
    }

    public async Task<Result> SendEnrolmentOfferReminder(
        Application application,
        Offer offer,
        string year,
        CancellationToken cancellationToken = default)
    {
        EnrolmentOfferReminderEmailViewModel viewModel = new()
        {
            Title = $"Assessment Submission Received",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            Id = offer.Id,
            Grade = application.Grade,
            ParentName = application.ParentName,
            StudentName = application.StudentName,
            RespondBy = offer.RespondBy.Value,
            Year = year
        };

        if (application.ParentName is null)
            return Result.Failure(ApplicationErrors.ArgumentNull(nameof(Application.ParentName)));

        if (application.ParentEmailAddress is null)
            return Result.Failure(ApplicationErrors.ArgumentNull(nameof(Application.ParentEmailAddress)));

        Result<EmailRecipient> recipient = EmailRecipient.Create(application.ParentName, application.ParentEmailAddress);

        if (recipient.IsFailure)
            return recipient;

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Enrolments",
            viewModel.Title,
            [recipient.Value],
            cancellationToken: cancellationToken);
    }
}