namespace Constellation.Application.Features.Faculties.Commands;

using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

public record UpdateFacultyCommand(
    Guid Id,
    string Name,
    string Colour
    ) : IRequest { }

public class UpdateFacultyCommandHandler : IRequestHandler<UpdateFacultyCommand>
{
    private readonly IAppDbContext _context;

    public UpdateFacultyCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateFacultyCommand request, CancellationToken cancellationToken)
    {
        var faculty = await _context.Faculties
            .SingleOrDefaultAsync(faculty => faculty.Id == request.Id, cancellationToken);

        if (faculty is not null)
        {
            faculty.Name = request.Name;
            faculty.Colour = request.Colour;

            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
