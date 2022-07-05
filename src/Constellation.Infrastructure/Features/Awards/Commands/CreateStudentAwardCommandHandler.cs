using Constellation.Application.Features.Awards.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Awards.Commands
{
    public class CreateStudentAwardCommandHandler : IRequestHandler<CreateStudentAwardCommand>
    {
        private readonly IAppDbContext _context;

        public CreateStudentAwardCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CreateStudentAwardCommand request, CancellationToken cancellationToken)
        {
            var entry = new StudentAward
            {
                StudentId = request.StudentId,
                Category = request.Category,
                Type = request.Type,
                AwardedOn = request.AwardedOn
            };

            _context.Add(entry);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
