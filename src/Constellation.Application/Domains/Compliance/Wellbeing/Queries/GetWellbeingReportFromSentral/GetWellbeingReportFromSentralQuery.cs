namespace Constellation.Application.Domains.Compliance.Wellbeing.Queries.GetWellbeingReportFromSentral;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetWellbeingReportFromSentralQuery()
    : IQuery<List<SentralIncidentDetails>>;