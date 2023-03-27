namespace Constellation.Application.Families.DeleteParentById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DeleteParentByIdCommandHandler
    : ICommandHandler<DeleteParentByIdCommand>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteParentByIdCommandHandler(
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork)
    {
        _familyRepository = familyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteParentByIdCommand request, CancellationToken cancellationToken)
    {
        var family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
        {
            return Result.Failure(DomainErrors.Families.Family.NotFound(request.FamilyId));
        }

        var parent = family.Parents.FirstOrDefault(parent => parent.Id == request.ParentId);

        if (parent is null)
        {
            return Result.Failure(DomainErrors.Families.Parents.NotFoundInFamily(request.ParentId, request.FamilyId));
        }

        family.RemoveParent(parent);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
