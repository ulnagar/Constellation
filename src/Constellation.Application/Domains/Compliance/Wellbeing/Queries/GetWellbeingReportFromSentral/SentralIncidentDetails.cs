namespace Constellation.Application.Domains.Compliance.Wellbeing.Queries.GetWellbeingReportFromSentral;

using Core.Enums;
using System;

public sealed record SentralIncidentDetails(
    string StudentReferenceNumber,
    DateOnly DateCreated,
    string IncidentId,
    string Subject,
    string Type,
    string Teacher,
    string StudentFirstName,
    string StudentLastName,
    Grade Grade,
    int Severity);