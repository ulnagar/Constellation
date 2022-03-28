using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Jobs.AbsenceMonitor.Command
{
    public class AddNewStudentAbsencesCommand : IRequest
    {
        public ICollection<Absence> Absences { get; set; }
    }

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
