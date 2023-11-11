namespace Constellation.Application.Compliance.GetWellbeingReportFromSentral;

using Abstractions.Messaging;
using DTOs;
using System.Collections.Generic;

public sealed record GetWellbeingReportFromSentralQuery()
    : IQuery<List<SentralIncidentDetails>>;