namespace Constellation.Application.Domains.EnrolmentContext.Applications.Queries.ExportApplicationsList;

using Abstractions.Messaging;
using System.Collections.Generic;
using ApplicationId = Core.Models.EnrolmentContext.Application.Identifiers.ApplicationId;

public sealed record ExportApplicationsListQuery(
    List<ApplicationId> ApplicationIds)
    : IQuery<byte[]>;