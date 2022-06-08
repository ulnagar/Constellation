using Constellation.Application.Features.ShortTerm.Covers.Commands;
using Constellation.Application.Features.ShortTerm.Covers.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.ShortTerm.Covers.Commands
{
    public class CancelCoverCommandHandler : IRequestHandler<CancelCoverCommand>
    {
        private readonly IAppDbContext _context;
        private readonly IMediator _mediator;

        public CancelCoverCommandHandler(IAppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(CancelCoverCommand request, CancellationToken cancellationToken)
        {
            var cover = await _context.Covers
                .FirstOrDefaultAsync(cover => cover.Id == request.CoverId, cancellationToken);

            cover.IsDeleted = true;

            await _context.SaveChangesAsync(cancellationToken);

            switch (cover)
            {
                case CasualClassCover casual:
                    await _mediator.Publish(new CasualCoverCancelledNotification { CoverId = request.CoverId });

                    break;
                case TeacherClassCover teacher:
                    await _mediator.Publish(new StaffCoverCancelledNotification { CoverId = request.CoverId });

                    break;
            }

            return Unit.Value;
        }
    }
}
