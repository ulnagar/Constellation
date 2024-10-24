namespace Constellation.Application.ThirdPartyConsent.GetRequiredApplicationsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetRequiredApplicationsForStudentQuery(
    StudentId StudentId)
    : IQuery<List<RequiredApplicationResponse>>;