namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Identifiers;
using Constellation.Infrastructure.Templates.Views.Emails.WorkFlow;
using Core.Extensions;
using Core.ValueObjects;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Action = Core.Models.WorkFlow.Action;

public sealed partial class Service : IEmailService
{
    public async Task SendActionAssignedEmail(
       List<EmailRecipient> recipients,
       Case item,
       Action action,
       StaffMember assignee,
       CancellationToken cancellationToken = default)
    {
        ActionAssignedEmailViewModel viewModel = new()
        {
            Title = $"[WorkFlow] Action Assigned",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            TeacherName = assignee.Name.DisplayName,
            ActionDescription = action.ToString(),
            CaseDescription = item.ToString(),
            CaseId = item.Id,
            ActionId = action.Id,
            LinkPart = $"/Staff/SchoolAdmin/WorkFlows/Actions/Update/{item.Id.Value}/{action.Id.Value}"
        };

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "WorkFlows",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }

    public async Task SendActionCancelledEmail(
        List<EmailRecipient> recipients,
        Case item,
        Action action,
        StaffMember assignee,
        CancellationToken cancellationToken = default)
    {
        ActionAssignedEmailViewModel viewModel = new()
        {
            Title = $"[WorkFlow] Action Cancelled",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            TeacherName = assignee.Name.DisplayName,
            ActionDescription = action.ToString(),
            CaseDescription = item.ToString(),
            CaseId = item.Id,
            ActionId = action.Id,
            LinkPart = $"/Staff/SchoolAdmin/WorkFlows/Actions/Update/{item.Id.Value}/{action.Id.Value}"
        };

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "WorkFlows",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }

    public async Task SendComplianceWorkFlowNotificationEmail(
        List<EmailRecipient> recipients,
        CaseId caseId,
        Name assignee,
        ComplianceCaseDetail detail,
        int incidentAge,
        string incidentLink,
        CancellationToken cancellationToken = default)
    {
        ComplianceWorkFlowNotificationEmailViewModel viewModel = new()
        {
            Title = $"[WorkFlow] Compliance Case Detected",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            Assignee = assignee.DisplayName,
            StudentName = detail.Name,
            StudentGrade = detail.Grade.AsName(),
            StudentSchool = detail.SchoolName,
            IncidentType = detail.IncidentType,
            IncidentId = detail.IncidentId,
            Subject = detail.Subject,
            IncidentLink = incidentLink,
            Age = incidentAge,
            LinkPart = $"/Staff/SchoolAdmin/WorkFlows/Details/{caseId.Value}"
        };

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "WorkFlows",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }

    public async Task SendTrainingWorkFlowNotificationEmail(
        List<EmailRecipient> recipients,
        TrainingCaseDetail detail,
        CancellationToken cancellationToken = default)
    {
        TrainingWorkFlowNotificationEmailViewModel viewModel = new()
        {
            Title = $"[WorkFlow] Mandatory Training Due",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            StaffName = detail.Name,
            ModuleName = detail.ModuleName,
            DueDate = detail.DueDate,
            DaysUntilDue = detail.DaysUntilDue
        };

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "WorkFlows",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }

    public async Task SendAllActionsCompletedEmail(
        List<EmailRecipient> recipients,
        Case item,
        CancellationToken cancellationToken = default)
    {
        CaseActionsCompletedEmailViewModel viewModel = new()
        {
            Title = $"[WorkFlow] All Case Actions Completed",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            CaseDescription = item.ToString(),
            LinkPart = $"/Staff/SchoolAdmin/WorkFlows/Details/{item.Id.Value}"
        };

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "WorkFlows",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }

    public async Task SendEnteredEmailForAction(
        List<EmailRecipient> recipients,
        EmailRecipient sender,
        string subject,
        string body,
        List<Attachment> attachments,
        CancellationToken cancellationToken = default) =>
        await BuildAndSendEmail(
            body,
            sender,
            subject,
            [],
            bccRecipients: recipients,
            attachments: attachments,
            cancellationToken: cancellationToken);
}
