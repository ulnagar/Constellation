namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Domains.AppSettings.Models;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Shared;
using Constellation.Infrastructure.Templates.Views.Emails.Enrolments;
using Core.Errors;
using Core.Models.AppSettings.Enums;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Enums;
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
        List<ContactsConfiguration> contacts = await _appSettings.Contacts(cancellationToken);

        ContactsConfiguration? principals = contacts.FirstOrDefault(entry => entry.Position == ContactPosition.Principal);

        string? principal = principals?.Contacts.FirstOrDefault(entry => entry.Value.Contains(application.Grade)).Key.Name.DisplayName;

        EnrolmentOfferNotificationEmailViewModel viewModel = new()
        {
            Title = $"Enrolment Offer",
            SenderName = principal ?? "",
            SenderTitle = "Principal",
            Preheader = "",
            Id = offer.Id,
            Grade = application.Grade,
            ParentName = application.ParentName,
            StudentName = application.StudentName,
            RespondBy = offer.RespondBy.Value,
            Year = year,
            Courses = application.SelectedCourses
                .Where(entry => entry.Status == CourseSelectionStatus.Approved)
                .Select(entry => entry.Course.Name)
                .ToList()
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
        List<ContactsConfiguration> contacts = await _appSettings.Contacts(cancellationToken);

        ContactsConfiguration? principals = contacts.FirstOrDefault(entry => entry.Position == ContactPosition.Principal);

        string? principal = principals?.Contacts.FirstOrDefault(entry => entry.Value.Contains(application.Grade)).Key.Name.DisplayName;

        EnrolmentOfferReminderEmailViewModel viewModel = new()
        {
            Title = $"Enrolment Offer Reminder",
            SenderName = principal ?? "",
            SenderTitle = "Principal",
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