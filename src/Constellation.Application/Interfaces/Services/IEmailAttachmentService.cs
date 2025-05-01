namespace Constellation.Application.Interfaces.Services;

using Core.Models.Offerings;
using Core.Models.Students;
using Core.Models.ThirdPartyConsent;
using Core.Models.Timetables;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

public interface IEmailAttachmentService
{
    Task<Attachment> GenerateClassRollDocument(Offering offering, List<Student> Students, CancellationToken cancellationToken = default);
    Task<Attachment> GenerateClassTimetableDocument(Offering offering, List<Period> relevantPeriods, CancellationToken cancellationToken = default);
    Task<Attachment> GenerateConsentTransactionReceipt(Transaction transaction, CancellationToken cancellationToken = default);
}
