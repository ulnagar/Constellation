namespace Constellation.Application.Domains.StudentOnboarding.Queries.DoesApplicantIdExist;

using Abstractions.Messaging;
using Core.Models.StudentOnboarding.Identifiers;

public sealed record DoesApplicantIdExistQuery(
    ApplicantId ApplicantId)
    : IQuery<bool>;