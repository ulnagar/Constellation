using Constellation.Application.Features.Jobs.AbsenceMonitor.Commands;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Jobs.AbsenceMonitor.Commands
{
    public class AddNewStudentAbsencesCommandHandler : IRequestHandler<AddNewStudentAbsencesCommand>
    {
        private readonly IAppDbContext _context;

        public AddNewStudentAbsencesCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(AddNewStudentAbsencesCommand request, CancellationToken cancellationToken)
        {
            await _context.Absences.AddRangeAsync(request.Absences, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
