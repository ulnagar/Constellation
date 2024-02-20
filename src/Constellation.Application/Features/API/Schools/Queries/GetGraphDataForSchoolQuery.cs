namespace Constellation.Application.Features.API.Schools.Queries;

using Constellation.Application.DTOs;
using MediatR;

public sealed class GetGraphDataForSchoolQuery : IRequest<GraphData>
{
    public string SchoolCode { get; set; }
    public int Day { get; set; }
}