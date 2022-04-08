using Constellation.Application.DTOs;
using MediatR;

namespace Constellation.Application.Features.API.Schools.Queries
{
    public class GetGraphDataForSchoolQuery : IRequest<GraphData>
    {
        public string SchoolCode { get; set; }
        public int Day { get; set; }
    }
}
