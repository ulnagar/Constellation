namespace Constellation.Application.WorkFlows.CreateComplianceCase;

using Abstractions.Messaging;
using Compliance.GetWellbeingReportFromSentral;

public sealed record CreateComplianceCaseCommand(
    SentralIncidentDetails Incident)
    : ICommand;