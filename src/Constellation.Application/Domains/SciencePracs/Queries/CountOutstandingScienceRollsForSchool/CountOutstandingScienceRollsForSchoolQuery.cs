namespace Constellation.Application.Domains.SciencePracs.Queries.CountOutstandingScienceRollsForSchool;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record CountOutstandingScienceRollsForSchoolQuery(
    SchoolCode SchoolCode)
    : IQuery<int>;