namespace Constellation.Application.Domains.ThirdPartyConsent.Queries.GetApplicationById;

using Abstractions.Messaging;
using Constellation.Core.Models.ThirdPartyConsent.Identifiers;

public sealed record GetApplicationByIdQuery(
    ApplicationId ApplicationId)
    : IQuery<ApplicationResponse>;