namespace Constellation.Application.Domains.WorkFlows.Commands.CreateComplianceCase;

using Abstractions.Messaging;
using Compliance.Wellbeing.Queries.GetWellbeingReportFromSentral;

public sealed record CreateComplianceCaseCommand(
    SentralIncidentDetails Incident)
    : ICommand;