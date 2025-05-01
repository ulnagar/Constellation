namespace Constellation.Application.Domains.ThirdPartyConsent.Queries.GetRequiredApplicationsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetRequiredApplicationsForStudentQuery(
    StudentId StudentId)
    : IQuery<List<RequiredApplicationResponse>>;