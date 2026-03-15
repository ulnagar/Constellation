namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Extensions;
using Constellation.Application.Domains.AppSettings.Models;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Errors;
using Core.Models.AppSettings.Enums;
using Core.Models.SchoolContacts;
using Core.Models.StaffMembers;
using Core.Shared;
using Core.ValueObjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Templates.Views.Emails.Contacts;

public sealed partial class Service : IEmailService
{
    public async Task SendWelcomeEmailToCoordinator(
        List<EmailRecipient> recipients,
        string schoolName,
        CancellationToken cancellationToken = default)
    {
        ContactsConfiguration? configuration = await _appSettings.Contacts(ContactPosition.InstructionalLeader, cancellationToken);

        if (configuration is null)
        {
            GetLogger()
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(ContactsConfiguration)), true)
                .Warning("Failed to send welcome email");

            return;
        }

        StaffMember instructionalLeader = configuration.Contacts.First().Key;
        Result<EmailRecipient> instructionalLeaderRecipient = instructionalLeader.GetEmailRecipient();

        if (instructionalLeaderRecipient.IsFailure)
        {
            GetLogger()
                .ForContext(nameof(Error), instructionalLeaderRecipient.Error, true)
                .Warning("Failed to send welcome email");

            return;
        }

        NewACCoordinatorEmailViewModel viewModel = new()
        {
            Title = $"Welcome to Aurora College!",
            SenderName = instructionalLeader.Name.DisplayName,
            SenderTitle = "Instructional Leader",
            Preheader = "",
            PartnerSchool = schoolName,
            InstructionalLeader = instructionalLeaderRecipient.Value
        };

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "School Contact",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }

    public async Task SendWelcomeEmailToSciencePracTeacher(
        List<EmailRecipient> recipients,
        string schoolName,
        CancellationToken cancellationToken = default)
    {
        LessonsConfiguration? configuration = await _appSettings.Lessons(cancellationToken);

        if (configuration is null)
        {
            GetLogger()
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(LessonsConfiguration)), true)
                .Warning("Failed to send welcome email");

            return;
        }

        Result<EmailRecipient> headTeacher = configuration.Contacts.First().Key.GetEmailRecipient();

        if (headTeacher.IsFailure)
        {
            GetLogger()
                .ForContext(nameof(Error), headTeacher.Error, true)
                .Warning("Failed to send welcome email");

            return;
        }

        NewSciencePracTeacherEmailViewModel viewModel = new()
        {
            Title = $"Welcome to Aurora College!",
            SenderName = configuration.CoordinatorName,
            SenderTitle = configuration.CoordinatorTitle,
            Preheader = "",
            PartnerSchool = schoolName,
            Coordinator = configuration.Recipient,
            HeadTeacher = headTeacher.Value
        };

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "School Contact",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }

    public async Task<Result> SendSchoolContactRemovalRequest(
        SchoolContact contact,
        SchoolContactRole role,
        string cancelledBy,
        string cancelledAt,
        string comment)
    {
        string viewModel = "<p>A school contact change has been requested:</p>";
        viewModel += $"<p><strong>{contact.DisplayName}</strong> should be removed as <strong>{role.Role}</strong> at <strong>{role.SchoolName}</strong></p>";
        viewModel += $"<p>This change was requested by <strong>{cancelledBy}</strong> on <strong>{cancelledAt}</strong> with the comment:</p>";
        viewModel += $"<p><strong>{comment}<strong></p>";

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "School Contact change requested",
            [EmailRecipient.InfoTechTeam, EmailRecipient.AuroraCollege, EmailRecipient.AbsencesMailbox]);
    }

    public async Task SendSchoolContactAddedNotification(
        SchoolContact contact,
        SchoolContactRole role)
    {
        string viewModel = "<p>A new school contact has been registered via the Schools Portal:</p>";
        viewModel += $"<p><strong>{contact.DisplayName}</strong> is the <strong>{role.Role}</strong> at <strong>{role.SchoolName}</strong></p>";
        viewModel += $"<p>This user was registered at <strong>{role.CreatedAt.ToLongDateString()}</strong>.";

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "New School Contact registered",
            [EmailRecipient.InfoTechTeam, EmailRecipient.AuroraCollege, EmailRecipient.AbsencesMailbox]);
    }
}
