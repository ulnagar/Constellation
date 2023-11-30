namespace Constellation.Application.Compliance.GetWellbeingReportFromSentral;

using Core.Enums;
using System;

public sealed record SentralIncidentDetails(
    string StudentId,
    DateOnly DateCreated,
    string IncidentId,
    string Subject,
    string Type,
    string Teacher,
    string StudentFirstName,
    string StudentLastName,
    Grade Grade,
    int Severity);