namespace Constellation.Application.Domains.StudentOnboarding.Queries.GetEnrolmentApplications;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetEnrolmentApplicationsQuery()
    : IQuery<List<EnrolmentApplicationResponse>>;