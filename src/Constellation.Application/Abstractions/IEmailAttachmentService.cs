﻿namespace Constellation.Application.Abstractions;

using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

public interface IEmailAttachmentService
{
    Task<Attachment> GenerateClassRollDocument(Offering offering, List<Student> Students, CancellationToken cancellationToken = default);
    Task<Attachment> GenerateClassTimetableDocument(Offering offering, List<Session> offeringSessions, List<TimetablePeriod> relevantPeriods, CancellationToken cancellationToken = default);
}
