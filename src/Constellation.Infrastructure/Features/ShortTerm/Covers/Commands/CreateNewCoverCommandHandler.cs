using Constellation.Application.Extensions;
using Constellation.Application.Features.ShortTerm.Covers.Commands;
using Constellation.Application.Features.ShortTerm.Covers.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var selectedClasses = new List<int>();

            if (!string.IsNullOrWhiteSpace(request.CoverDto.TeacherId))
            {
                var cycleDays = new List<int>();
                foreach (var day in request.CoverDto.StartDate.Range(request.CoverDto.EndDate))
                {
                    cycleDays.Add(day.GetDayNumber());
                }

                var offerings = await _context.Sessions
                    .Where(session => session.StaffId == request.CoverDto.TeacherId && cycleDays.Contains(session.Period.Day) && !session.IsDeleted && session.Offering.EndDate >= DateTime.Today)
                    .Select(session => session.OfferingId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                selectedClasses.AddRange(offerings);
            }

            if (request.CoverDto.SelectedClasses != null)
            {
                selectedClasses.AddRange(request.CoverDto.SelectedClasses.Distinct());
            }

            if (casualCover && !staffCover)
            {
                // This is a casualCover
                var conversion = int.TryParse(request.CoverDto.UserId, out int intId);

                if (conversion)
                {
                    foreach (var @class in selectedClasses)
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
                foreach (var @class in selectedClasses)
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
