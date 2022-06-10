using MediatR;

namespace Constellation.Application.Features.ShortTerm.Covers.Commands
{
    public class CancelCoverCommand : IRequest
    {
        public int CoverId { get; set; }
    }
}
