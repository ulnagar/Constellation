namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Infrastructure.Templates.Views.Emails.AwardNominations;
using Constellation.Infrastructure.Templates.Views.Emails.Awards;
using Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

public sealed partial class Service : IEmailService
{
    public async Task SendAwardCertificateParentEmail(
        List<EmailRecipient> recipients,
        Attachment certificate,
        StudentAward award,
        Student? student,
        StaffMember? teacher,
        CancellationToken cancellationToken = default)
    {
        NewAwardCertificateEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = "",
            SenderTitle = "",
            Title = $"[Aurora College] Student Award for {student.Name.DisplayName}",
            AwardType = award.Type,
            AwardedOn = award.AwardedOn,
            AwardReason = award.Reason,
            StudentName = student?.Name.DisplayName,
            TeacherName = teacher?.Name.DisplayName
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Awards/NewAwardCertificateEmail.cshtml", viewModel);

        foreach (EmailRecipient recipient in recipients)
        {
            await _emailSender.Send([recipient], EmailRecipient.NoReply.Email, viewModel.Title, body, new List<Attachment> { certificate }, cancellationToken);
        }
    }

    public async Task<Result<string>> SendAwardNominationNotificationEmailToSchools(
    List<EmailRecipient> recipients,
    List<EmailRecipient> ccRecipients,
    Name contact,
    string school,
    DateOnly deliveryDate,
    Dictionary<Name, List<Nomination>> students,
    CancellationToken cancellationToken = default)
    {
        SchoolNotificationEmailViewModel viewModel = new()
        {
            Title = $"Student Awards - {school}",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            Contact = contact,
            School = school,
            DeliveryDate = deliveryDate,
            Students = students
        };

        string body = await _razorService.RenderViewToStringAsync(SchoolNotificationEmailViewModel.ViewLocation, viewModel);

        var emailSendOperation = await _emailSender.Send(recipients, ccRecipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);

        if (emailSendOperation.IsFailure)
            return Result.Failure<string>(emailSendOperation.Error);

        return body;
    }

    public async Task<Result<string>> SendAwardNominationNotificationEmailToParents(
        List<EmailRecipient> recipients,
        List<EmailRecipient> ccRecipients,
        Name parent,
        Name student,
        string school,
        DateOnly deliveryDate,
        List<Nomination> awards,
        CancellationToken cancellationToken = default)
    {
        ParentNotificationEmailViewModel viewModel = new()
        {
            Title = $"Student Awards - {student.DisplayName}",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            Parent = parent,
            Student = student,
            School = school,
            DeliveryDate = deliveryDate,
            Awards = awards
        };

        string body = await _razorService.RenderViewToStringAsync(ParentNotificationEmailViewModel.ViewLocation, viewModel);

        var emailSendOperation = await _emailSender.Send(recipients, ccRecipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);

        if (emailSendOperation.IsFailure)
            return Result.Failure<string>(emailSendOperation.Error);

        return body;
    }
}
