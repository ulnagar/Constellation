namespace Constellation.Application.Domains.StudentOnboarding.Queries.DoesApplicationIdExist;

using Abstractions.Messaging;
using Core.Models.StudentOnboarding.Identifiers;

public sealed record DoesApplicationIdExistQuery(
    ApplicationId ApplicationId)
    : IQuery<bool>;