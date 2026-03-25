namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Attachments.DTOs;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Infrastructure.Templates.Views.Emails.AwardNominations;
using Constellation.Infrastructure.Templates.Views.Emails.Awards;
using Core.Models.Messaging.Email;
using Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

public sealed partial class Service : IEmailService
{
    public async Task SendAwardCertificateParentEmail(
        List<EmailRecipient> recipients,
        AttachmentResponse certificate,
        StudentAward award,
        Student student,
        StaffMember teacher,
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
            StudentName = student.Name.DisplayName,
            TeacherName = teacher.Name.DisplayName
        };
        
        foreach (EmailRecipient recipient in recipients)
        {
            MemoryStream stream = new(certificate.FileData);

            using Attachment attachment = new(stream, certificate.FileName, certificate.FileType);

            await BuildAndSendEmail(
                viewModel,
                EmailRecipient.NoReply,
                "Awards",
                viewModel.Title,
                [recipient],
                attachments: [attachment],
                cancellationToken: cancellationToken);
        }
    }

    public async Task<Result<EmailMessage>> SendAwardNominationNotificationEmailToSchools(
        List<EmailRecipient> recipients,
        List<EmailRecipient> ccRecipients,
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
            School = school,
            DeliveryDate = deliveryDate,
            Students = students
        };

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Awards",
            viewModel.Title,
            recipients,
            ccRecipients: ccRecipients,
            cancellationToken: cancellationToken);
    }

    public async Task<Result<EmailMessage>> SendAwardNominationNotificationEmailToParents(
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

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Awards",
            viewModel.Title,
            recipients,
            ccRecipients: ccRecipients,
            cancellationToken: cancellationToken);
    }
}
