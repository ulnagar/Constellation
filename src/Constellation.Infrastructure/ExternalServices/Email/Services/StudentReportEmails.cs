namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Core.ValueObjects;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Templates.Views.Emails.Reports;

public sealed partial class Service : IEmailService
{
    public async Task SendAcademicReportToNonResidentialParent(
        List<EmailRecipient> recipients,
        Name studentName,
        string reportingPeriod,
        string year,
        FileDto file,
        CancellationToken cancellationToken = default)
    {
        List<Attachment> attachments = new();
        MemoryStream stream = new(file.FileData);
        attachments.Add(new Attachment(stream, file.FileName, file.FileType));

        foreach (EmailRecipient parent in recipients)
        {
            AcademicReportEmailViewModel viewModel = new()
            {
                Preheader = "",
                SenderName = "Chris Robertson",
                SenderTitle = "Principal",
                Title = $"[Aurora College] Academic Report Published",
                ParentName = parent.Name,
                StudentName = studentName,
                ReportingPeriod = reportingPeriod,
                Year = year
            };

            await BuildAndSendEmail(
                viewModel,
                EmailRecipient.AuroraCollege,
                "Student Reports",
                viewModel.Title,
                recipients,
                attachments: attachments,
                cancellationToken: cancellationToken);
        }
    }
}
