namespace Constellation.Application.Domains.ExternalSystems.NetworkStatistics.Queries.GetGraphDataForSchool;

using DTOs;
using MediatR;

public sealed class GetGraphDataForSchoolQuery : IRequest<GraphData>
{
    public string SchoolCode { get; set; }
    public int Day { get; set; }
}