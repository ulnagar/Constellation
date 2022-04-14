using MediatR;
using System;

namespace Constellation.Application.Features.API.Operations.Commands
{
    public class CreateTeamCommand : IRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ChannelId { get; set; }
    }
}
