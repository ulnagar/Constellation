namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetAssessmentProvisionById;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Models;

public sealed record GetAssessmentProvisionByIdQuery(
    ProvisionId Id)
    : IQuery<AssessmentProvisionResponse>;