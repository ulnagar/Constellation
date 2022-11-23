namespace Constellation.Application.Features.Faculties.Commands;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record CreateFacultyCommand(
    string Name,
    string Colour
    ) : IRequest { }

public class CreateFacultyCommandHandler : IRequestHandler<CreateFacultyCommand>
{
    private readonly IAppDbContext _context;

    public CreateFacultyCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(CreateFacultyCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Faculties.AnyAsync(faculty => !faculty.IsDeleted && faculty.Name == request.Name, cancellationToken);

        if (!exists)
        {
            var faculty = new Faculty
            {
                Name = request.Name,
                Colour = request.Colour
            };

            _context.Faculties.Add(faculty);
            await _context.SaveChangesAsync(cancellationToken);
        }
        
        return Unit.Value;
    }
}
