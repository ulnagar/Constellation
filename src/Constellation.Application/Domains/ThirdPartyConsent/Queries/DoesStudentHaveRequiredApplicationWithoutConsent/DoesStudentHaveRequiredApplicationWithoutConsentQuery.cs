namespace Constellation.Application.Domains.ThirdPartyConsent.Queries.DoesStudentHaveRequiredApplicationWithoutConsent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record DoesStudentHaveRequiredApplicationWithoutConsentQuery(
    StudentId StudentId)
    : IQuery<bool>;
