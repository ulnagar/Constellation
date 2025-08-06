namespace Constellation.Application.Domains.Compliance.Assessments.Models;

using Core.Enums;
using Core.Models.Offerings.Identifiers;
using Core.Models.Offerings.ValueObjects;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;
using System.Text;

public sealed record StudentProvisions(
    StudentId StudentId,
    Name Student,
    Grade Grade,
    List<StudentProvisions.OfferingProvision> OfferingProvisions)
{
    public EmailStatus StudentEmail { get; internal set; } = EmailStatus.Pending;
    public EmailStatus SchoolEmail { get; internal set; } = EmailStatus.Pending;

    public enum EmailStatus
    {
        Pending,
        Sent,
        Error
    }

    public sealed record OfferingProvision(
        OfferingId OfferingId,
        string CourseName,
        OfferingName Name,
        string ExamName,
        List<string> Provisions);

    public string FormatAdjustments()
    {
        StringBuilder builder = new();
        builder.AppendLine("<dl>");

        foreach (var offering in OfferingProvisions)
        {
            builder.AppendLine($"<dt>{offering.Name} - {offering.CourseName}<br /><em>{offering.ExamName}</em></dt>");
            builder.AppendLine("<dd>");

            foreach (var provision in offering.Provisions)
            {
                builder.AppendLine($"{provision}<br />");
            }

            builder.AppendLine("</dd>");
        }
        
        builder.AppendLine("</dl>");

        return builder.ToString();
    }
}
