using Constellation.Application.Features.ShortTerm.Covers.Commands;
using Constellation.Application.Features.ShortTerm.Covers.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.ShortTerm.Covers.Commands
{
    public class CreateNewCoverCommandHandler : IRequestHandler<CreateNewCoverCommand>
    {
        private readonly IAppDbContext _context;
        private readonly IMediator _mediator;

        public CreateNewCoverCommandHandler(IAppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(CreateNewCoverCommand request, CancellationToken cancellationToken)
        {
            // Is this a casual or staff cover?
            var casualCover = request.CoverDto.UserType == "Casuals";
            var staffCover = request.CoverDto.UserType == "Teachers";

            if ((staffCover && casualCover) || (!staffCover && !casualCover))
            {
                // Somethings broken
            }

            if (casualCover && !staffCover)
            {
                // This is a casualCover
                var conversion = int.TryParse(request.CoverDto.UserId, out int intId);

                if (conversion)
                {
                    foreach (var @class in request.CoverDto.SelectedClasses)
                    {
                        var entry = new CasualClassCover
                        {
                            CasualId = intId,
                            OfferingId = @class,
                            StartDate = request.CoverDto.StartDate,
                            EndDate = request.CoverDto.EndDate
                        };

                        _context.Add(entry);
                        await _context.SaveChangesAsync(cancellationToken);

                        await _mediator.Publish(new CasualCoverCreatedNotification { CoverId = entry.Id }, cancellationToken);
                    }
                }
            }

            if (staffCover && !casualCover)
            {
                // This is a staffCover
                foreach (var @class in request.CoverDto.SelectedClasses)
                {
                    var entry = new TeacherClassCover
                    {
                        StaffId = request.CoverDto.UserId,
                        OfferingId = @class,
                        StartDate = request.CoverDto.StartDate,
                        EndDate = request.CoverDto.EndDate
                    };

                    _context.Add(entry);
                    await _context.SaveChangesAsync(cancellationToken);

                    await _mediator.Publish(new StaffCoverCreatedNotification { CoverId = entry.Id }, cancellationToken);
                }
            }

            return Unit.Value;
        }
    }
}
