namespace Constellation.Application.Families.DeleteFamilyById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DeleteFamilyByIdCommandHandler
    : ICommandHandler<DeleteFamilyByIdCommand>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFamilyByIdCommandHandler(
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork)
    {
        _familyRepository = familyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteFamilyByIdCommand request, CancellationToken cancellationToken)
    {
        var family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
        {
            return Result.Failure(DomainErrors.Families.Family.NotFound(request.FamilyId));
        }

        family.Delete();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
