namespace Constellation.Application.Families.RemoveStudentFromFamily;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveStudentFromFamilyCommandHandler
    : ICommandHandler<RemoveStudentFromFamilyCommand>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveStudentFromFamilyCommandHandler(
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork)
    {
        _familyRepository = familyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveStudentFromFamilyCommand request, CancellationToken cancellationToken)
    {
        var family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
            return Result.Failure(DomainErrors.Families.Family.NotFound(request.FamilyId));

        var result = family.RemoveStudent(request.StudentId);

        if (result.IsSuccess)
        {
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return result;
    }
}
