namespace Constellation.Application.Domains.Tutorials.Requests.Queries.GetAllTutorialRequests;

using Core.Models.Tutorials.Enums;
using Core.Models.Tutorials.Identifiers;
using Core.ValueObjects;
using System;

public sealed record TutorialRequestSummaryResponse(
    RequestId RequestId,
    Name Student,
    TutorialType Type,
    string Subject,
    RequestStatus Status,
    DateOnly ActionDate);