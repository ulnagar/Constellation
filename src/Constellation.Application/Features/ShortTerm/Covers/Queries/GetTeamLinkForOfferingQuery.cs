using MediatR;

namespace Constellation.Application.Features.ShortTerm.Covers.Queries
{
    public class GetTeamLinkForOfferingQuery : IRequest<string>
    {
        public string ClassName { get; set; }
        public string Year { get; set; }
    }
}
