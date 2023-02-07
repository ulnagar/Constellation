namespace Constellation.Application.Abstractions;

using Constellation.Core.Models;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

public interface IEmailAttachmentService
{
    Task<Attachment> GenerateClassRollDocument(CourseOffering offering, List<Student> Students, CancellationToken cancellationToken = default);
    Task<Attachment> GenerateClassTimetableDocument(CourseOffering offering, List<OfferingSession> offeringSessions, List<TimetablePeriod> relevantPeriods, CancellationToken cancellationToken = default);
}
