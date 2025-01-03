﻿namespace Constellation.Application.Abstractions;

using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Students;
using Core.Models.ThirdPartyConsent;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

public interface IEmailAttachmentService
{
    Task<Attachment> GenerateClassRollDocument(Offering offering, List<Student> Students, CancellationToken cancellationToken = default);
    Task<Attachment> GenerateClassTimetableDocument(Offering offering, List<TimetablePeriod> relevantPeriods, CancellationToken cancellationToken = default);
    Task<Attachment> GenerateConsentTransactionReceipt(Transaction transaction, CancellationToken cancellationToken = default);
}
