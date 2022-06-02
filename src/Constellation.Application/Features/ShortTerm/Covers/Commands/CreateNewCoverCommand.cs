using Constellation.Application.DTOs;
using MediatR;

namespace Constellation.Application.Features.ShortTerm.Covers.Commands
{
    public class CreateNewCoverCommand : IRequest
    {
        public CoverDto CoverDto { get; set; }
    }
}
