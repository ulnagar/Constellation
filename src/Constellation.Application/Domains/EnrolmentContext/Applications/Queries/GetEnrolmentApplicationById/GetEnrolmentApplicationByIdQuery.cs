namespace Constellation.Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationById;

using Abstractions.Messaging;
using ApplicationId = Core.Models.EnrolmentContext.Application.Identifiers.ApplicationId;

public sealed record GetEnrolmentApplicationByIdQuery(
    ApplicationId Id)
    : IQuery<EnrolmentApplicationResponse>;