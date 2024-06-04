namespace Constellation.Application.Faculties.CreateFaculty;

using Abstractions.Messaging;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateFacultyCommandHandler 
    :ICommandHandler<CreateFacultyCommand>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFacultyCommandHandler(IFacultyRepository facultyRepository, IUnitOfWork unitOfWork)
    {
        _facultyRepository = facultyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateFacultyCommand request, CancellationToken cancellationToken)
    {
        bool exists = await _facultyRepository.ExistsWithName(request.Name, cancellationToken);

        if (!exists)
        {
            Faculty faculty = new(
                request.Name,
                request.Colour);

            _facultyRepository.Insert(faculty);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success();
    }
}