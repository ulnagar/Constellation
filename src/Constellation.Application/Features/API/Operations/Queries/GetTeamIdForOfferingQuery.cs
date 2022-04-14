using MediatR;

namespace Constellation.Application.Features.API.Operations.Queries
{
    public class GetTeamIdForOfferingQuery : IRequest<string>
    {
        public string ClassName { get; set; }
        public string Year { get; set; }
    }
}
