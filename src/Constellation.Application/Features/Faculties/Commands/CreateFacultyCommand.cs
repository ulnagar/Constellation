namespace Constellation.Application.Features.Faculties.Commands;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public record CreateFacultyCommand(
    string Name,
    string Colour
    ) : IRequest { }

public class CreateFacultyCommandHandler : IRequestHandler<CreateFacultyCommand>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFacultyCommandHandler(IFacultyRepository facultyRepository, IUnitOfWork unitOfWork)
    {
        _facultyRepository = facultyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CreateFacultyCommand request, CancellationToken cancellationToken)
    {
        var exists = await _facultyRepository.ExistsWithName(request.Name, cancellationToken);

        if (!exists)
        {
            var faculty = new Faculty
            {
                Name = request.Name,
                Colour = request.Colour
            };

            _facultyRepository.Insert(faculty);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }
        
        return Unit.Value;
    }
}
