namespace Constellation.Application.Domains.ExternalSystems.NetworkStatistics.Queries.GetGraphDataForSchool;

using Core.Models.Identifiers;
using DTOs;
using MediatR;

public sealed class GetGraphDataForSchoolQuery : IRequest<GraphData>
{
    public SchoolCode SchoolCode { get; set; }
    public int Day { get; set; }
}