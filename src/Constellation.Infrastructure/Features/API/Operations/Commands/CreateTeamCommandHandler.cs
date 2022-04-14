using Constellation.Application.Features.API.Operations.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Constellation.Infrastructure.Features.API.Operations.Commands
{
    public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand>
    {
        private readonly IAppDbContext _context;

        public CreateTeamCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
        {
            var checkTeam = await _context.Teams.FirstOrDefaultAsync(team => team.Id == request.Id);

            if (checkTeam != null)
                return Unit.Value;

            var team = new Team
            {
                Id = request.Id,
                Name = request.Name,
                Link = $"https://teams.microsoft.com/l/team/{HttpUtility.UrlEncode(request.ChannelId)}/conversations?groupId={HttpUtility.UrlEncode(request.Id.ToString())}&tenantId={HttpUtility.UrlEncode("05a0e69a-418a-47c1-9c25-9387261bf991")}"
            };

            _context.Add(team);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
